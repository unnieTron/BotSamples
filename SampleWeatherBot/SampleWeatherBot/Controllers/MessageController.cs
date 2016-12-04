using Microsoft.Bot.Connector;
using SampleWeatherBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace SampleWeatherBot.Controllers
{
    [BotAuthentication]
    public class MessageController : ApiController
    {
        // POST api/<controller>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                var city = await CallWeatherApi(activity.Text);
                var weather = city.weather.FirstOrDefault();
                var reply = activity.CreateReply($@"The weather for 
                            {city.name} today is {weather.main},{weather.description} 
                            with temperature between {city.main.temp_min} 
                            and {city.main.temp_max}");
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                await HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }
        private async Task<Activity> HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
                var connector = new ConnectorClient(new Uri(message.ServiceUrl));
                // return our reply to the user
                var reply = message.CreateReply($"Hi {message.From.Name}.Please enter city name eg:London");
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            return null;
        }

        private async Task<CityWeather> CallWeatherApi(string cityName)
        {
            CityWeather city = null;
            var client = new HttpClient()
            {
                BaseAddress = new Uri($"http://api.openweathermap.org/")
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await client.GetAsync($"data/2.5/weather?q={cityName}&units=metric&APPID=66a1007c8ee2eda8482a842b14c4457a");
            if (response.IsSuccessStatusCode)
            {
                city = await response.Content.ReadAsAsync<CityWeather>();
            }
            return city;
        }
    }
}