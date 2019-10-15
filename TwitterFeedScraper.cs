using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using STFeedS.Enums;

namespace STFeedS
{
   public class TwitterFeedScraper
    {
        private HtmlWeb web { get; set; }

        public TwitterFeedScraper()
        {
            web = new HtmlWeb();
        }

        public async Task<bool> ValidateTwitterPage(string twitterUserName)
        {

            var url = "https://twitter.com/" + twitterUserName;
            //doc = web.Load("https://twitter.com/" + twitterUserName);
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        public DateTime ConvertTwitterDateTime(string twitterDate)
        {
            var dateFormatProvider = CultureInfo.InvariantCulture;
            var dateParts = twitterDate.Split(' ');

            if (dateParts.Length == 2)
            {
                //creationDateString += $" {DateTime.Now.Year.ToString()}";

                twitterDate = dateParts[1] + " " + dateParts[0] + " " + DateTime.Now.Year.ToString();

            }

            DateTime resultDate;

            try
            {
                resultDate = DateTime.ParseExact(twitterDate, "d MMM yyyy", dateFormatProvider);
            }
            catch (Exception)
            {
                resultDate = new DateTime(2000, 1, 1);


            }
            return resultDate;
        }

        public TwitterFeedPage GetTwitterPage(string twitterUrl)
        {
            var resultTweetPage = new TwitterFeedPage();
            //resultTweetPage.ListOfTweets = new List<Tweet>();
            HtmlDocument doc = web.Load(twitterUrl);

            var tablesDivNode = doc.DocumentNode.SelectSingleNode("/html[1]/body[1]/div[1]/div[3]/div[3]");
            var nextPageTweetNode = doc.DocumentNode.SelectSingleNode($"/html[1]/body[1]/div[1]/div[3]/div[3]/div[1]/a[1]");

            resultTweetPage.NextPageUrl = "https://twitter.com/" + nextPageTweetNode.Attributes["href"].Value;


            var tables = tablesDivNode.Elements("table");
            //Debug.WriteLine($"BEFORE THE TABLES LOOP. twitterUrl: {twitterUrl}; nextpage: {resultTweetPage.NextPageUrl}; TABLES COUNT: {tables.Count()}");
            for (int i = 2; i < tables.Count() + 1; i++)
            {
                Tweet tmpTweet = new Tweet();

                try
                {
                    var tweetNode = doc.DocumentNode.SelectSingleNode($"/html[1]/body[1]/div[1]/div[3]/div[3]/table[{i}]/tr[2]/td[1]/div[1]/div[1]");
                    tmpTweet.Text = tweetNode.InnerText;

                    var creationDateString = doc.DocumentNode.SelectSingleNode($"/html[1]/body[1]/div[1]/div[3]/div[3]/table[{i}]/tr[1]/td[3]/a[1]").InnerText;

                    var links = tweetNode.Elements("a");

                    var cleanTweetText = tweetNode.InnerText;
                    if (links.Any())
                    {

                        foreach (var item in links)
                        {
                            if (item.Attributes["href"].Value.Contains("https://t.co/") && !item.Attributes["data-url"].Value.Contains("https://twitter.com/"))
                            {

                                tmpTweet.Links.Add(item.Attributes["href"].Value);

                                cleanTweetText = cleanTweetText.Replace(item.InnerText, "");
                            }
                        }
                    }

                    tmpTweet.CleanText = cleanTweetText;




                    var creationDate = ConvertTwitterDateTime(creationDateString);
                    tmpTweet.creationDate = creationDate;




                    resultTweetPage.ListOfTweets.Add(tmpTweet);

                }
                catch (Exception)
                {

                    try
                    {
                        var tweetNode = doc.DocumentNode.SelectSingleNode($"/html[1]/body[1]/div[1]/div[3]/div[3]/table[{i}]/tr[3]/td[1]/div[1]/div[1]");
                        tmpTweet.Text = tweetNode.InnerText;

                        var creationDateString = doc.DocumentNode.SelectSingleNode($"/html[1]/body[1]/div[1]/div[3]/div[3]/table[{i}]/tr[2]/td[3]/a[1]").InnerText;


                        var cleanTweetText = tweetNode.InnerText;



                        var links = tweetNode.Elements("a");//.Element("a")
                        if (links.Any())
                        {

                            foreach (var item in links)
                            {
                                if (item.Attributes["href"].Value.Contains("https://t.co/") && !item.Attributes["data-url"].Value.Contains("https://twitter.com/"))
                                {
                                    tmpTweet.Links.Add(item.Attributes["href"].Value);

                                    cleanTweetText = cleanTweetText.Replace(item.InnerText, "");
                                }
                            }
                        }

                        tmpTweet.CleanText = cleanTweetText;

                        var creationDate = ConvertTwitterDateTime(creationDateString);
                        tmpTweet.creationDate = creationDate;
                        tmpTweet.TweetType = TweetType.Retweet;

                        resultTweetPage.ListOfTweets.Add(tmpTweet);



                    }
                    catch (Exception)
                    {


                    }
                }

            }


            return resultTweetPage;
        }

        public List<Tweet> GetTweetsByTimePeriod(string twitterUsername, DateTime fromDate, DateTime toDate)
        {
            var result = new List<Tweet>();

            DateTime lastTweetDate = toDate;

            var twitterUrl = "https://twitter.com/" + twitterUsername;

            bool isFirstItemFound = false;

            var tmpTweets = GetTwitterPage(twitterUrl);
            //Debug.WriteLine($"TMPTWEETS GOT FIRST TIME. COUNT: {tmpTweets.ListOfTweets.Count}. Next page url: {tmpTweets.NextPageUrl}");
            while (!isFirstItemFound)
            {
                //Debug.WriteLine($"IN WHILE; COUNT: {tmpTweets.ListOfTweets.Count}. Next page url: {tmpTweets.NextPageUrl}");

                if (tmpTweets.ListOfTweets.LastOrDefault().creationDate <= toDate)
                {
                    isFirstItemFound = true;
                }
                else
                {
                    tmpTweets = GetTwitterPage(tmpTweets.NextPageUrl);

                }
            }



            bool lastResultFound = false;
            if (tmpTweets.ListOfTweets.LastOrDefault().creationDate >= fromDate)
            {
                result.AddRange(tmpTweets.ListOfTweets.Where(t => t.creationDate >= fromDate || t.creationDate <= toDate));

                lastResultFound = true;
            }




            while (!lastResultFound)
            {
                foreach (var tweet in tmpTweets.ListOfTweets)
                {
                    if (tweet.creationDate >= fromDate)
                    {
                        result.Add(tweet);
                    }
                    else
                    {
                        lastResultFound = true;
                    }

                }
                if (!lastResultFound)
                {
                    tmpTweets = GetTwitterPage(tmpTweets.NextPageUrl);
                }


            }



            return result;
        }




    }
}
