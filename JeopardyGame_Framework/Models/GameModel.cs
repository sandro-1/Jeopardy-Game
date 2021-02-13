using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using static JeopardyGame_Framework.Models.ClueComponentModels;
using static JeopardyGame_Framework.Models.RoundModels;
using HtmlAgilityPack;
using System.Data.SqlClient;

namespace JeopardyGame_Framework.Models
{
    public class GameModel
    {
        public int ID { get; set; } 
        public bool GameFinished { get; set; }
        public string ShowNumber { get; set; } 
        public string JArchiveGameId { get; set; } 
        public string ShowDebutDate { get; set; } 
        public string GameScore { get; set; } 
        public string UserEmail { get; set; }
        public string GameTally { get; set; } 
        public string DatePlayed { get; set; }
        public Jeopardy JRound { get; set; }
        public DoubleJeopardy DJRound { get; set; }
        public FinalJeopardy FJRound { get; set; }

        public GameModel()
        {
            JRound = new Jeopardy();
            DJRound = new DoubleJeopardy();
            FJRound = new FinalJeopardy();            
        }

        public static List<string> ShuffleList(List<string> inputList)
        {
            Random rnd = new Random();
            int n = inputList.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                string value = inputList[k];
                inputList[k] = inputList[n];
                inputList[n] = value;
            }
            return inputList;
        }

        public static async Task<List<BaseGameModel>> CollectBaseGameInformation()
        {
            List<BaseGameModel> baseGameModelList = new List<BaseGameModel>();

            //season 16 - 35
            for (int f = 16; f < 35; f++)
            {
                var seasonNumber = f.ToString(); 

                var url = "http://www.j-archive.com/showseason.php?season=" + seasonNumber;
                var httpClient = new HttpClient();
                var html = await httpClient.GetStringAsync(url);

                Regex rgxShowNumber = new Regex("(>&#35;)");
                string showNumberClean = rgxShowNumber.Replace(html, "xxshow_number=");
                Regex rgxDate = new Regex("(, aired&#160;)");
                string dateClean = rgxDate.Replace(showNumberClean, "xxdate=");

                //GameModel_id=6079"xxshow_number=7815xxdate=2018-07-27</a>

                for (int i = 0; i < dateClean.Length - 19; i++)
                {
                    var showNumber = "";
                    var gameId = "";
                    var date = "";
                    var j = i;

                    var a = '"'.ToString();

                    if (dateClean.Substring(i, 8) == "game_id=")
                    {                    
                        while (dateClean.Substring(j + 8, 1) != '"'.ToString()) //collect gameId
                        {
                            gameId += dateClean.Substring(j + 8, 1);
                            j++;
                        }
                        while (dateClean.Substring(j, 14) != "xxshow_number=") //collect showNumber
                        {
                            j++;
                        }                    
                        while (dateClean.Substring(j + 14, 7) != "xxdate=") //collect showDebutDate 
                        {
                            showNumber += dateClean.Substring(j + 14, 1);
                            j++;
                        }                    
                        if (dateClean.Substring(j + 14, 7) == "xxdate=")
                        {
                            date = dateClean.Substring(j + 21, 10);
                        }
                        BaseGameModel newBaseGameModel = new BaseGameModel();
                        newBaseGameModel.BaseGameJArchiveGameId = gameId;
                        newBaseGameModel.BaseGameShowNumber = showNumber;
                        newBaseGameModel.BaseGameShowDebutDate = date;
                        baseGameModelList.Add(newBaseGameModel);
                    }
                }
            }
            return baseGameModelList;
        }

        public static async Task<GameModel> PlayAsync(string randomOrReplay)
        {
            var url = "";
            var showId = "";
            var gameId = "";
            var ShowDebutDate = "";
            var increment = 0;
            var db = ApplicationDbContext.Create();


            if (randomOrReplay == "random")
            {
                //range 3446 - 7815 (inclusive)
                Random rnd = new Random();             
                var randomShowNumber = rnd.Next(3446, 7816);
                showId = randomShowNumber.ToString();
                gameId = "";

                //SELECT BaseGameJArchiveGameId FROM[aspnet - JeopardyGame_Framework - 20190131083158].[dbo].[BaseGameModels] WHERE BaseGameShowNumber = 3555
                //Data Source=(LocalDb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\aspnet-JeopardyGame_Framework-20190131083158.mdf;Initial Catalog=aspnet-JeopardyGame_Framework-20190131083158;Integrated Security=True" providerName="System.Data.SqlClient
                string queryString = "SELECT BaseGameJArchiveGameId FROM [aspnet-JeopardyGame_Framework-20190131083158].[dbo].[BaseGameModels] WHERE BaseGameShowNumber = " + showId;
                string connectionString = "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=aspnet-JeopardyGame_Framework-20190131083158;Integrated Security=True";

                /*"Data Source=ServerName;" + "Initial Catalog=DataBaseName;" + "Integrated Security=SSPI;"*/;
                string test = "";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    try
                    {
                        while (reader.Read())
                        {
                            test = reader.GetString(0);
                        }
                    }
                    finally
                    {                        
                        reader.Close();
                    }
                }

                gameId = test;

                //while ("" == gameId)
                //{
                //    if (showId == db.BaseGames.AsEnumerable().ElementAt(increment).BaseGameShowNumber)
                //    {
                //        gameId = db.BaseGames.AsEnumerable().ElementAt(increment).BaseGameJArchiveGameId;
                //        ShowDebutDate = db.BaseGames.AsEnumerable().ElementAt(increment).BaseGameShowDebutDate;
                //    }
                //    increment++;
                //}

                url = "http://www.j-archive.com/showgame.php?game_id=" + gameId;
            }
            else
            {
                url = "http://www.j-archive.com/showgame.php?game_id=" + randomOrReplay;
            }

            //collect html string from jeopardy website
            var httpClient = new HttpClient();

            var html = await httpClient.GetStringAsync(url);

            //load html into agility pack to organize html
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            //collect positions and clues for every question
            var clues = htmlDocument.DocumentNode.Descendants("td").
                        Where(node => node.GetAttributeValue("class", "").
                        Equals("clue_text"));

            //declare a new GameModel          
            GameModel newGameModel = new GameModel();
            //split positions/clues into jeopardy, doublejeopardy, and finaljeopardy objects  
            newGameModel.ShowNumber = showId;
            newGameModel.JArchiveGameId = gameId;
            newGameModel.ShowDebutDate = ShowDebutDate;
            newGameModel.GameScore = "0";
            newGameModel.JRound.JeopardyScore = 0;
            newGameModel.DJRound.DoubleJeopardyScore = 0;

            //for error handling
            db.Games.Add(newGameModel);
            db.SaveChanges();

            int jCount = 0;
            int djCount = 0;
            Regex removeAmp = new Regex("(&amp;)");
            Regex searchLinks = new Regex("(a href)"); //search for a href
            List<bool> linkListJ = new List<bool>();
            List<bool> linkListDJ = new List<bool>();
            
            for (int i = 0; i < clues.Count(); i++)
            {
                string cleanClue = removeAmp.Replace(clues.ElementAt(i).InnerText, "and");
                bool links = searchLinks.IsMatch(clues.ElementAt(i).InnerHtml);
                
                if (clues.ElementAt(i).Id.Length == 7)
                {
                    if (links == true)
                    {
                        var gameModel = await PlayAsync(randomOrReplay); //call playasync again and restart because finaljeopardy wont be playable if this is the case                        
                        return gameModel;
                    }
                    newGameModel.FJRound.FJClue = cleanClue;
                    continue;
                }
                else if (clues.ElementAt(i).Id.Length < 11)
                {
                    if (links == true)
                    {
                        linkListJ.Add(true);
                    }
                    else
                    {
                        linkListJ.Add(false);
                    }

                    Clue newClue = new Clue();
                    newGameModel.JRound.JClues.Add(newClue);
                    Position newPosition = new Position();
                    newGameModel.JRound.JPositions.Add(newPosition);
                    newGameModel.JRound.JClues.ElementAt(jCount).ClueText = cleanClue;
                    newGameModel.JRound.JPositions.ElementAt(jCount).CluePosition = clues.ElementAt(i).Id;
                    jCount++;
                    continue;
                }
                else
                {
                    if (links == true)
                    {
                        linkListDJ.Add(true);
                    }
                    else
                    {
                        linkListDJ.Add(false);
                    }

                    Clue newClueDJ = new Clue();
                    newGameModel.DJRound.DJClues.Add(newClueDJ);
                    Position newPositionDJ = new Position();
                    newGameModel.DJRound.DJPositions.Add(newPositionDJ);
                    newGameModel.DJRound.DJClues.ElementAt(djCount).ClueText = cleanClue;
                    newGameModel.DJRound.DJPositions.ElementAt(djCount).CluePosition = clues.ElementAt(i).Id;
                    djCount++;
                }
            }

            //find the answers by parsing the html, add them to list
            //int p = 26; //correct_response&quot;&gt;
            //int p2 = 35; //correct_response&quot;&gt;&lt;i&gt;
            //int p3 = 27; //correct_response\&quot;&gt;
            List<string> answers = new List<string>();

            Regex rgx = new Regex("(i&gt;)|(\\\\)|(gt;)|('\\))|(;em)|(;br)|(&quot;)|(/em)|(&gt;)|(/br)|(&lt)|(&gt)|[&;/]");
            string cleanHtml = rgx.Replace(html, "");

            string answer = "";
            for (int i = 0; i < cleanHtml.Length - 16; i++)
            {
                if (cleanHtml.Substring(i, 16) == "correct_response")
                {
                    int p = 16;
                    while (cleanHtml.Substring(i + p, 5) != "table" && cleanHtml.Substring(i + p, 1) != "\"")
                    {
                        answer += cleanHtml.Substring(i + p, 1);
                        p++;
                    }
                    if (answer.Trim().ToLower() == "=")
                    {
                        answer = "";
                        continue;
                    }
                    //unicode/lowercase/grammer parse
                    string answerInput = answer.Trim().ToLower();

                    Regex punctuationParse = new Regex("[\\p{P}]");
                    string punctuationParsedOutput = punctuationParse.Replace(answerInput, "");
                    Regex phraseParse = new Regex("\\A(1 of )|\\A(a )|\\A(an )|\\A(the )");
                    string phraseParsedOutput = phraseParse.Replace(punctuationParsedOutput, "");
                    Regex ampParse = new Regex("( amp )|( or )");
                    string ampParsedOutput = ampParse.Replace(phraseParsedOutput, " ");
                    Regex unicodeParse = new Regex("[^0-9a-z\\s]");
                    string fullyParsedOutput = unicodeParse.Replace(ampParsedOutput, "[a-z]");
                    answers.Add(fullyParsedOutput);
                    answer = "";
                    continue;
                }
            }

            //pass answer into jeopardy, dj, and fj objects
            for (int i = 0; i < newGameModel.JRound.JClues.Count; i++)
            {
                Answer newAnswer = new Answer();
                newGameModel.JRound.JAnswers.Add(newAnswer);
                newGameModel.JRound.JAnswers.ElementAt(i).ClueAnswer = answers.ElementAt(i);
            }
            int DJListPosition = newGameModel.JRound.JClues.Count;
            for (int i = 0; i < newGameModel.DJRound.DJClues.Count; i++)
            {
                Answer newAnswerDJ = new Answer();
                newGameModel.DJRound.DJAnswers.Add(newAnswerDJ);
                newGameModel.DJRound.DJAnswers.ElementAt(i).ClueAnswer = answers.ElementAt(DJListPosition);
                DJListPosition++;
            }
            newGameModel.FJRound.FJAnswer = answers.ElementAt(answers.Count - 1);


            //find category nodes
            var categories = htmlDocument.DocumentNode.Descendants("td").
                    Where(node => node.GetAttributeValue("class", "").
                    Equals("category"));

            //pass categories into GameModel object
            for (int a = 0; a < categories.Count() - 1; a++)
            {
                if (a < 6)
                {
                    string cleanCategory = removeAmp.Replace(categories.ElementAt(a).InnerText.Trim(), "AND");
                    Category newCategory = new Category();
                    newCategory.CategoryName = cleanCategory;
                    newGameModel.JRound.JCategories.Add(newCategory);
                }
                else
                {
                    string cleanCategoryDJ = removeAmp.Replace(categories.ElementAt(a).InnerText.Trim(), "AND");
                    Category newCategoryDJ = new Category();
                    newCategoryDJ.CategoryName = cleanCategoryDJ;
                    newGameModel.DJRound.DJCategories.Add(newCategoryDJ);
                }
            }
            string cleanCategoryFJ = removeAmp.Replace(categories.ElementAt(categories.Count() - 1).InnerText.Trim(), "AND");
            newGameModel.FJRound.FJCategory = cleanCategoryFJ;

            //find value/dailydouble nodes
            var values = htmlDocument.DocumentNode.Descendants("td").
                Where(node => node.GetAttributeValue("class", "").
                Equals("clue_value"));
            //var dailyDoubles = htmlDocument.DocumentNode.Descendants("td").
            //    Where(node => node.GetAttributeValue("class", "").
            //    Equals("clue_value_daily_double"));

            //attribute value to JList.JeopardyValue/DJList.JeopardyValue variables
            int m = 0;
            for (int i = 0; i < newGameModel.JRound.JClues.Count; i++)
            {
                string id = newGameModel.JRound.JPositions.ElementAt(i).CluePosition;
                string htmlElement = values.ElementAt(i - m).ParentNode.InnerHtml;
                bool match = false;
                int length = htmlElement.Length - id.Length;
                if (id == htmlElement.Substring(21, id.Length))
                {
                    match = true;
                    Value newValue = new Value();
                    newGameModel.JRound.JValues.Add(newValue);
                    newGameModel.JRound.JValues.ElementAt(i).ClueValue = values.ElementAt(i - m).InnerText;
                }
                if (match == false)
                {
                    Value newValueDD = new Value();
                    newGameModel.JRound.JValues.Add(newValueDD);
                    if (id.Substring(9, 1) == "1")
                    {
                        newGameModel.JRound.JValues.ElementAt(i).ClueValue = "$200DD";
                    }
                    else if (id.Substring(9, 1) == "2")
                    {
                        newGameModel.JRound.JValues.ElementAt(i).ClueValue = "$400DD";
                    }
                    else if (id.Substring(9, 1) == "3")
                    {
                        newGameModel.JRound.JValues.ElementAt(i).ClueValue = "$600DD";
                    }
                    else if (id.Substring(9, 1) == "4")
                    {
                        newGameModel.JRound.JValues.ElementAt(i).ClueValue = "$800DD";
                    }
                    else if (id.Substring(9, 1) == "5")
                    {
                        newGameModel.JRound.JValues.ElementAt(i).ClueValue = "$1000DD";
                    }
                    m++;
                }
            }
            for (int i = 0; i < newGameModel.DJRound.DJClues.Count; i++)
            {
                string id = newGameModel.DJRound.DJPositions.ElementAt(i).CluePosition;
                string htmlElement = values.ElementAt(newGameModel.JRound.JClues.Count + i - m).ParentNode.InnerHtml;
                bool match = false;
                int length = htmlElement.Length - id.Length;
                if (id == htmlElement.Substring(21, id.Length))
                {
                    match = true;
                    Value newValueDJ = new Value();
                    newGameModel.DJRound.DJValues.Add(newValueDJ);
                    newGameModel.DJRound.DJValues.ElementAt(i).ClueValue = values.ElementAt(newGameModel.JRound.JClues.Count + i - m).InnerText;
                }
                if (match == false)
                {
                    Value newValueDJDD = new Value();
                    newGameModel.DJRound.DJValues.Add(newValueDJDD);
                    if (id.Substring(10, 1) == "1")
                    {
                        newGameModel.DJRound.DJValues.ElementAt(i).ClueValue = "$400DD";
                    }
                    else if (id.Substring(10, 1) == "2")
                    {
                        newGameModel.DJRound.DJValues.ElementAt(i).ClueValue = "$800DD";
                    }
                    else if (id.Substring(10, 1) == "3")
                    {
                        newGameModel.DJRound.DJValues.ElementAt(i).ClueValue = "$1200DD";
                    }
                    else if (id.Substring(10, 1) == "4")
                    {
                        newGameModel.DJRound.DJValues.ElementAt(i).ClueValue = "$1600DD";
                    }
                    else if (id.Substring(10, 1) == "5")
                    {
                        newGameModel.DJRound.DJValues.ElementAt(i).ClueValue = "$2000DD";
                    }
                    m++;
                }
                if (i == newGameModel.DJRound.DJClues.Count - 2 && m == 2) //handling for situations where the bottom right most clue is a dd
                {
                    m++;
                }
            }

            //removes questions with hyperlinks because they reference pictures
            for (int i = 0; i < newGameModel.JRound.JClues.Count; i++)
            {
                if (linkListJ.ElementAt(i) == true)
                {
                    newGameModel.JRound.JClues.RemoveAt(i);
                    newGameModel.JRound.JAnswers.RemoveAt(i);
                    newGameModel.JRound.JPositions.RemoveAt(i);
                    newGameModel.JRound.JValues.RemoveAt(i);
                }
            }
            for (int i = 0; i < newGameModel.DJRound.DJClues.Count; i++)
            {
                if (linkListDJ.ElementAt(i) == true)
                {
                    newGameModel.DJRound.DJClues.RemoveAt(i);
                    newGameModel.DJRound.DJAnswers.RemoveAt(i);
                    newGameModel.DJRound.DJPositions.RemoveAt(i);
                    newGameModel.DJRound.DJValues.RemoveAt(i);
                }
            }
            return newGameModel;
        }
    }
}