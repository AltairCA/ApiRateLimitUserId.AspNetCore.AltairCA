using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ApiRateLimiterUserIdExample;
using ApiRateLimiterUserIdTest.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ApiRateLimiterUserIdTest
{
    public class HeaderTest: IClassFixture<CustomWebApplicationFactory<ApiRateLimiterUserIdExample.Startup>>
    {
        private readonly CustomWebApplicationFactory<ApiRateLimiterUserIdExample.Startup> _factory;
        private const string ClearURL = "/api/values";

        public const string AuthToken =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwNTg3YzkyYS02N2M0LTRjMmQtODZlOS02ZDQ2NTFlYWM4NzEiLCJqdGkiOiJjMWZmOTI1Zi03YzgwLTQ2MDktYmIyNi0yMmRiODlkOGYwNDQiLCJpYXQiOjE2MTQ3NTA3MzUsIm5iZiI6MTYxNDc1MDczNSwiZXhwIjoxNjE1MTgyNzM1LCJpc3MiOiJBbHRhaXJDQSIsImF1ZCI6IkFsdGFpckNBQXVkaWVuY2UifQ.X9PkMWzVBOVuUNVSUp4EYHEZwl5VQe1MPlzwK-28KIk";
        public HeaderTest(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("/api/values/2",1)]
        [InlineData("/api/values", 9)]
        public async Task Response_Contain_Remaining_Header(string url,int remainingCount)
        {
            //Arrange
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);
            await client.DeleteAsync(ClearURL);

            //Act
            var response = await client.GetAsync(url);
            

            //Assert
            response.EnsureSuccessStatusCode();

            var limit_remainingHeader = response.Headers.FirstOrDefault(x => x.Key == "x-rate-limit-remaining");

            Assert.NotNull(limit_remainingHeader);

            var limit_remainingValue = limit_remainingHeader.Value.FirstOrDefault();

            Assert.NotNull(limit_remainingValue);
            Assert.Equal(remainingCount,Convert.ToInt32(limit_remainingValue));
        }
        [Theory]
        [InlineData("/api/values/2", 1)]
        [InlineData("/api/values", 9)]
        public async Task Response_Reach_Limit_0(string url, int remainingCount)
        {
            //Arrange
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);
            await client.DeleteAsync(ClearURL);


            while (remainingCount >= 0)
            {
                //Act
                var response = await client.GetAsync(url);


                //Assert
                response.EnsureSuccessStatusCode();

                var limit_remainingHeader = response.Headers.FirstOrDefault(x => x.Key == "x-rate-limit-remaining");

                Assert.NotNull(limit_remainingHeader);

                var limit_remainingValue = limit_remainingHeader.Value.FirstOrDefault();

                Assert.NotNull(limit_remainingValue);
                Assert.Equal(remainingCount, Convert.ToInt32(limit_remainingValue));
                remainingCount--;
            }
            
        }
    }
}
