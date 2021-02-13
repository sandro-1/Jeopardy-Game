using JeopardyGame_Framework.Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static JeopardyGame_Framework.Models.RoundModels;

namespace JeopardyGame_Framework.Controllers
{
    [Authorize]
    public class GameController : Controller
    {
        private static ApplicationDbContext _db = ApplicationDbContext.Create();
        private static GameModel game; //collects original model when playasync is called, resets back to that original everytime another controller is called         

        //Jeopardy Page
        //string randomOrReplay, string email (parameters)
        public async Task<ActionResult> JeopardyRound() //randomorreplay passed through url selection, email passed through sigin model
        {
            //add base games!!!!!!!!!!!!!!!
            //List<BaseGameModel> baseGameList = await GameModel.CollectBaseGameInformation();

            //for (int i = 0; i < baseGameList.Count; i++)
            //{
            //    _db.BaseGames.Add(baseGameList.ElementAt(i));
            //}

            //_db.SaveChanges();

            //the place where a game is called will need handling the passes random or an already saved gameId
            var signInManager = HttpContext.GetOwinContext().Get<ApplicationSignInManager>(); //get signInManager
            var user = signInManager.AuthenticationManager.User.Identity.Name; //isolate current user
            
            game = await GameModel.PlayAsync("random");
            game.UserEmail = user;
            game.DatePlayed = DateTime.Now.ToString("MM/dd/yyyy");
            game.JRound.testId = game.JArchiveGameId; //for testing
            game.DJRound.testId = game.JArchiveGameId; //for testing
            
            return View(game.JRound);
        }
        
        //Double Jeopardy Page        
        public async Task<ActionResult> DoubleJeopardyRound() 
        {            
            var currentGame = _db.Games.AsEnumerable().Last();
         
            game.DJRound.DoubleJeopardyScore = currentGame.DJRound.DoubleJeopardyScore;
            
            return View(game.DJRound);
        }

        ////Final Jeopardy Page
        public async Task<ActionResult> FinalJeopardyRound() /*string fjScore, string fjNumCorrect*/
        {

            var currentGame = _db.Games.AsEnumerable().Last();
            game.FJRound.FinalJeopardyScore = currentGame.FJRound.FinalJeopardyScore;
            
            return View(game.FJRound);
        }

        public async Task<ActionResult> GamelogHomepage()
        {
            var signInManager = HttpContext.GetOwinContext().Get<ApplicationSignInManager>(); //get signInManager
            var user = signInManager.AuthenticationManager.User.Identity.Name; //isolate current user

            ////put hyperlink urls on shownumbers listed here, direct urls to controller below
            List<GameModel> gameList = new List<GameModel>();

            for (int i = 0; i < _db.Games.Count(); i++)
            {
                if (_db.Games.AsEnumerable().ElementAt(i).UserEmail == user) //checks whether current users id equals db saved user ids
                {
                    if (_db.Games.AsEnumerable().ElementAt(i).GameFinished != true) //checks whether the game was finished
                    {
                        _db.Games.Remove(_db.Games.AsEnumerable().ElementAt(i));
                        _db.SaveChanges();
                    } else
                    {
                        gameList.Add(_db.Games.AsEnumerable().ElementAt(i));
                    }
                }
            }

            gameList.Reverse();

            return View(gameList);
        }

        [HttpPost]
        public ActionResult TransitionPost(string score, string round, string tally)
        {   
            if (round == "JeopardyRound") //if jeopardyround gets passed, it means jeopardyround is over and its time to transition to djround
            {
                game.DJRound.DoubleJeopardyScore = Int32.Parse(score);
                game.GameTally = tally;
                _db.Games.Add(game);
                _db.SaveChanges();
                return Json(Url.Action("DoubleJeopardyRound", "Game"));
            }
            else if (round == "DoubleJeopardyRound")
            {
                var currentGame = _db.Games.AsEnumerable().Last();
                currentGame.FJRound.FinalJeopardyScore = Int32.Parse(score);
                currentGame.GameTally = AdjustTally(tally, currentGame.GameTally);
                _db.SaveChanges();
                if (Int32.Parse(score) < 1)
                {                    
                    currentGame.GameFinished = true; //game finished, not going to double jeopardy
                    currentGame.GameScore = score;
                    _db.SaveChanges();
                    return Json(Url.Action("GamelogHomepage", "Game"));
                    }
                return Json(Url.Action("FinalJeopardyRound", "Game"));
            }
            else
            {
                var currentGame = _db.Games.AsEnumerable().Last();
                currentGame.GameFinished = true; //game finished full
                currentGame.GameScore = score;
                currentGame.GameTally = AdjustTally(tally, currentGame.GameTally);  
                return Json(Url.Action("GamelogHomepage", "Game"));
            }
        }

        public string AdjustTally(string ajaxTallyInput, string databaseTallyInput)
        {
            string tallyOutput;

            var ajaxFirstIndex = ajaxTallyInput.IndexOf("/");
            var databaseFirstIndex = databaseTallyInput.IndexOf("/");

            var ajaxCorrect = Int32.Parse(ajaxTallyInput.Substring(0, ajaxFirstIndex));
            var databaseCorrect = Int32.Parse(databaseTallyInput.Substring(0, databaseFirstIndex));

            tallyOutput = (ajaxCorrect + databaseCorrect).ToString();

            var ajaxRemainder = ajaxTallyInput.Substring(ajaxFirstIndex + 1);
            var databaseRemainder = databaseTallyInput.Substring(databaseFirstIndex + 1);

            var ajaxIndexTwo = ajaxRemainder.IndexOf("/");
            var databaseIndexTwo = databaseRemainder.IndexOf("/");

            var ajaxIncorrect = Int32.Parse(ajaxRemainder.Substring(0, ajaxIndexTwo));
            var databaseIncorrect = Int32.Parse(databaseRemainder.Substring(0, databaseIndexTwo));

            tallyOutput += "/" + (ajaxIncorrect + databaseIncorrect).ToString();

            var ajaxUnanswered = Int32.Parse(ajaxRemainder.Substring(ajaxIndexTwo + 1));
            var databaseUnanswered = Int32.Parse(databaseRemainder.Substring(databaseIndexTwo + 1));

            tallyOutput += "/" + (ajaxUnanswered + databaseUnanswered).ToString();

            return tallyOutput;
        }
    }
}