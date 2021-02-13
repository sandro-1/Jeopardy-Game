using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JeopardyGame_Framework.Models
{
    public class ClueComponentModels
    {
        public class Category
        {
            public int ID { get; set; }
            public string CategoryName { get; set; }
        }
        
        public class Answer
        {
            public int ID { get; set; }
            public string ClueAnswer { get; set; }
        }

        public class Clue
        {
            public int ID { get; set; }
            public string ClueText { get; set; }
        }

        public class Position
        {
            public int ID { get; set; }
            public string CluePosition { get; set; }
        }

        public class Value
        {
            public int ID { get; set; }
            public string ClueValue { get; set; }
        }
    }
}