using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static JeopardyGame_Framework.Models.ClueComponentModels;

namespace JeopardyGame_Framework.Models
{
    public class RoundModels
    {
        public class Jeopardy
        {
            public int ID { get; set; }
            public int JeopardyScore { get; set; }
            public List<Position> JPositions { get; set; }
            public List<Clue> JClues { get; set; }
            public List<Value> JValues { get; set; }
            public List<Answer> JAnswers { get; set; }
            public List<Category> JCategories { get; set; }
            public string testId; //for testing

            public Jeopardy()
            {
                JPositions = new List<Position>();
                JClues = new List<Clue>();
                JValues = new List<Value>();
                JAnswers = new List<Answer>();
                JCategories = new List<Category>();
            }
        }

        public class DoubleJeopardy
        {
            public int ID { get; set; }
            public int DoubleJeopardyScore { get; set; }
            public List<Position> DJPositions { get; set; }
            public List<Clue> DJClues { get; set; }
            public List<Value> DJValues { get; set; }
            public List<Answer> DJAnswers { get; set; }
            public List<Category> DJCategories { get; set; }
            public string testId; //for testing

            public DoubleJeopardy()
            {
                DJPositions = new List<Position>();
                DJClues = new List<Clue>();
                DJValues = new List<Value>();
                DJAnswers = new List<Answer>();
                DJCategories = new List<Category>();
            }
        }

        public class FinalJeopardy
        {
            public int ID { get; set; }
            public int FinalJeopardyScore { get; set; }
            public string FJClue { get; set; }
            public string FJAnswer { get; set; }
            public string FJCategory { get; set; }
        }

        //used for logging all relevant GameModel info. logged values plugged into playasync to find relevant GameModel
        public class BaseGameModel
        {
            public int ID { get; set; }
            public string BaseGameShowNumber { get; set; }
            public string BaseGameJArchiveGameId { get; set; } //shares this name with gamemodel
            public string BaseGameShowDebutDate { get; set; }
        }
    }
}