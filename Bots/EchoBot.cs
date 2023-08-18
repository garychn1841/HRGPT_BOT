// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with EchoBot .NET Template version v4.17.1

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Azure.AI.OpenAI;
using Azure;
using System.Linq;
using StackExchange.Redis;
using System.Net.Http;
using System.Text.Json;
using NRedisStack;
using NRedisStack.RedisStackCommands;
//using Microsoft.ApplicationInsights;

// for example
namespace EchoBot.Bots
{

    public class EchoBot : ActivityHandler
    {
        //Azure OpenAI 參數設定
        readonly string endpoint = "";       //your Azure base
        readonly string key = "";            //your Azure key
        readonly string deploymentName = ""; //your Azure chat model 
        private readonly OpenAIClient _embeddingClient;

        /* Hold a connection to a Redis server.*/
        private static readonly Lazy<ConnectionMultiplexer> LazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            // Replace with your Redis server connection string
            var options = ConfigurationOptions.Parse("");    //your ACI FQDN, ex: FQDN:port
            options.Password = "";                           //your redis password 
            return ConnectionMultiplexer.Connect(options);
        });


        /* This property provides a convenient way to access the connection to the Redis server.*/
        public static ConnectionMultiplexer Connection => LazyConnection.Value;

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            //宣告Azure OpenAI API
            var client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));

            //宣告及建立redis-stack連結
            var redisConnection = LazyConnection.Value;
            IDatabase redisDatabase = redisConnection.GetDatabase();
            SearchCommands ft = redisDatabase.FT();

            //宣告對話輸入及輸出
            var userInput = turnContext.Activity.Text.Trim();
            var modelOutput = string.Empty;
            
           
            //使用者輸入對話 Embedding
            EmbeddingsOptions embeddingOptions = new(userInput);
            var returnValue = client.GetEmbeddings("ada", embeddingOptions);
            var queryVector = returnValue.Value.Data[0].Embedding.SelectMany(BitConverter.GetBytes).ToArray();

            /* A message is sent to the user indicating that the bot is generating a response. */
            await turnContext.SendActivityAsync("start generating...");

            //redis-search
            int topN = 10;
            var query = $"*=>[KNN {topN} @search_vector $vector AS vector_score]";
            var redis_result = ft.Search("HR", new NRedisStack.Search.Query(query)
                                                                     .AddParam("vector", queryVector)
                                                                     .SetSortBy("vector_score")
                                                                     .Dialect(2));
            //redis-seach 結果處理
            string content="";
            string source="";
            int num=0;
            foreach (var result in redis_result.Documents)
            {
                content = content + result["page_content"];
                source = source + $"第{num}筆" +result["source"].ToString().Replace("./PDF/", "");
                num++;
            }

            //OpenAI ChatModel 對話設定
            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                Messages =
                {
                    new ChatMessage(ChatRole.System, $@"你是新光金控公司內部最資深的問答機器人，您的目的是回答用戶的所有問題，使用這系統的人皆是新光金融公司內部的員工，請用熱情的口語以及繁體中文回答。
                                                       回答時，目標是以清晰、詳細、信息豐富且提供範例的方式提供答案。詢問者詢問的問題，請詳細描述與詢問問題相關的解答以及提供範例。
                                                       如果問題問得太模糊沒有方向，請提供最相似的答案回覆，並自動進一步的追問是否有解答問題。除非另有說明，否則以句子格式提供回應。
                                                       最後面請參考{source}:[]裡面的資料來源，以'資料來源：'為標題在全部的回不最後一行單獨生成你看的參考資料是來自哪裡，例如：資料來源：新光金融控股股份有限公司工作規則_1110208.pdf。
                                                       不要回答不在數據庫範圍內的問題。如果詢問找不到的內容，請善意地回答並說:“內容不足，無法生成”，或者可以提供相近的依據條例，讓使用者參考。
                                                       Question: “{userInput}”
                                                       Text: <{content}>
                                                       Source: [{source}]")
                },
                MaxTokens = 5000,
                Temperature = (float)0.0

            };


            /* 將使用者對話加進目前對話中 */
            chatCompletionsOptions.Messages.Add(new ChatMessage(ChatRole.User, userInput));

          
            /* AI model is called with the provided options. */
            Response<ChatCompletions> response = await client.GetChatCompletionsAsync(
                deploymentOrModelName: deploymentName,
                chatCompletionsOptions);

            ChatCompletions ChatCompletions = response.Value;

            modelOutput = ChatCompletions.Choices[0].Message.Content;

            //將生成的對話送出至對話視窗
            await turnContext.SendActivityAsync(MessageFactory.Text(modelOutput), cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "這是第二版，歡迎使用新光金控內部問題問答機器人，您可以向我詢問企業內部常見問題，如：公司對哪些專業證照有津貼?持股信託怎麼算?婚喪喜慶有補助嗎?等，輸入【結束】則結束對話。";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}



