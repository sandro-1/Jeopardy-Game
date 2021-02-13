//question: made up of 4 parts that include the position, clue, value, and answer.
//questionId: a unique number ascribed to each question. each of the questions parts will shared this id.
//set: keyword describing a part grouping. because sets are groupings of parts, the 4 sets are position, clue, value, or answer.
//id = questionId-set: the id will be a combination questionId and set. questionId-set allows the targeting of the specific part of whatever question.

var globalQuestionTime = 20;
var globalTimerElement;
var globalY; //holds timer's timeout function
var globalScoreElement = document.getElementById("score");
var globalBetValue;
var globalTransitionCounter = 0; //used to monitor transition from jeopardy to dj and from dj to fj
var globalPageId = document.getElementById("pageId").innerText; //clarifies round the game is in 
var globalCorrect = 0;
var globalIncorrect = 0;
var globalUnanswered = 0;
var globalTally;
var globalTransition = false;
var globalClueListCount = 0;

if (window.history && window.history.pushState) {    
    $(window).on('popstate', function () { //called whenever active history entry changes                
        window.location.href = "http://localhost:50550/Game/GamelogHomepage";
        alert('As the directions outlined, cannot go backwards or refresh midgame, else the game will be deleted.');                
    });
    window.history.pushState('back', null, "http://localhost:50550/Game/GamelogHomepage");
}

function finalJeopardyTimer() {    
    globalTimerElement = document.getElementById("timer");
    globalTimerElement.className = "";
    globalTimerElement.innerText = globalQuestionTime; //decrements html value as timer ticks
    globalQuestionTime -= 1;
    globalY = setTimeout(finalJeopardyTimer, 1000);
    if (globalQuestionTime == -1) {
        globalUnanswered++;
        clearTimeout(globalY);
        globalTally = globalCorrect + "/" + globalIncorrect + "/" + globalUnanswered;
        transitionCheck(); //bet time runs out
    }
}

function finalJeopardyBetCheck(betElement) {
    clearTimeout(globalY);
    globalQuestionTime = 30;
    globalBetValue = parseInt(betElement.value);
    if (globalBetValue <= parseInt(globalScoreElement.innerText.substring(globalScoreElement.innerText.indexOf("$") + 1))) {
        globalScoreElement.innerText = globalScoreElement.innerText.substring(globalScoreElement.innerText.indexOf("$") + 1);
        globalScoreElement.className = "hidden";
        document.getElementById("category").className = "hidden";
        document.getElementById("betBox").className = "hidden";
        document.getElementById("clue").className = "";
        document.getElementById("guessBox").className = "";
        finalJeopardyTimer();
    } else {
        alert("You can't bet more than you have you total donkey.")
        globalQuestionTime = 20;
        finalJeopardyTimer();
    }
}

function finalJeopardyAnswerCheck(guessElement) {
    clearTimeout(globalY);
    var answer = document.getElementById("answer").innerText;
    document.getElementById("userGuess").innerText = guessElement.value; //inputing guess value into userGuess
    
    if (check(guessElement.value, answer) == true) { //if answer is correct
        document.getElementById("marked").innerText = "Correct";
        document.getElementById("marked").style.backgroundColor = "limegreen";       
        globalCorrect++;
        document.getElementById("clue").className = "hidden";
        globalScoreElement.innerText = (parseInt(globalScoreElement.innerText) + globalBetValue).toString();
        document.getElementById("result").innerText = "Correct!";
        document.getElementById("guessBox").className = "hidden";
        document.getElementById("timer").className = "hidden";
        setTimeout(transitionCheck, 2000);
    } else { //if answer is incorrect
        document.getElementById("marked").innerText = "Incorrect";
        document.getElementById("marked").style.backgroundColor = "tomato";
        globalIncorrect++;
        document.getElementById("clue").className = "hidden";
        globalScoreElement.innerText = (parseInt(globalScoreElement.innerText) - globalBetValue).toString();
        document.getElementById("result").innerText = "Incorrect!";
        document.getElementById("guessBox").className = "hidden";
        document.getElementById("timer").className = "hidden";
        setTimeout(transitionCheck, 2000);
    }
}

function regularValueClick(id) {
    var valueElement = document.getElementById(id);
    var questionId = valueElement.id.substring(0, valueElement.id.indexOf("-")); //removes the set from the id. use questionId to find clueElement.
    document.getElementById(questionId + "-clue").className = ""; //finds/shows clueElement
    document.getElementById(questionId + "-guessBox").className = ""; //finds/shows, guessBox
    globalTimerElement = document.getElementById(questionId + "-timer");
    valueElement.className = "hidden";
    document.addEventListener("click", handler, true); //calls handler, which freezes all click events to keep user from clicking on multiple questions at once
    globalTimerElement.className = ""; //show timer
    timer(); //call timer function           
}

//shows dailyDoubleBetBox and bet timer
function dailyDoubleValueClick(id) {
    var dailyDoubleValueQuestionId = id.substring(0, id.indexOf("-"));
    document.getElementById(id).className = "hidden"; //hide dailyDoubleValueElement
    document.getElementById(dailyDoubleValueQuestionId + "-dailyDoubleBet").className = ""; //show dailyDoubleBetElement
    globalTimerElement = document.getElementById(dailyDoubleValueQuestionId + "-timer");
    document.getElementById(dailyDoubleValueQuestionId + "-timer").className = "";
    document.addEventListener("click", handler, true); //calls handler, which freezes all click events to keep user from clicking on multiple questions at once
    timer();
}

//captures betValue, shows clue and guessBox, triggers timer
function dailyDoubleBet(id) {
    clearTimeout(globalY);
    globalQuestionTime = 20;
    var betValue = document.getElementById(id).value;
    if (betValue > 1000 && parseInt(globalScoreElement.innerText) < betValue) {
        alert("You can't bet more than $1000 you donkey!");
        dailyDoubleValueClick(id);
    } else if (betValue <= 0) {
        alert("You can't bet negative/nothing you firehose!");
        dailyDoubleValueClick(id);
    } else {
        var dailyDoubleBetQuestionId = id.substring(0, id.indexOf("-"));
        globalTimerElement = document.getElementById(dailyDoubleBetQuestionId + "-timer");
        document.getElementById(id).className = "hidden"; //hides betBox
        document.getElementById(dailyDoubleBetQuestionId + "-clue").className = "";
        document.getElementById(dailyDoubleBetQuestionId + "-guessBox").className = "";
        globalBetValue = betValue;
        document.addEventListener("click", handler, true); //calls handler, which freezes all click events to keep user from clicking on multiple questions at once
        globalTimerElement.className = ""; //show timer
        timer();
    }
}

//checks whether guess matches answer, adjusts score accordingly. checks answer for regular elements and daily doubles.
function checkAnswer(guessElement) {
    var guessValue = guessElement.value.toLowerCase();
    var guessId = guessElement.id.substring(0, guessElement.id.indexOf("-")); //removes set from id. use guessId to find answer.
    var answerInnerText = document.getElementById(guessId + "-answer").innerText; //isolate answer from answerElement
    var valueInnerText;
    var scoreInnerText = parseInt(document.getElementById("score").innerText); //collect score, parseInt      

    document.getElementById(guessId + "-userGuess").innerText = guessValue; //log guesses into userGuess

    //handling for dailydouble
    if (document.getElementById(guessId + "-value") == null) { //if null, this is a daily double
        valueInnerText = parseInt(globalBetValue);
        document.getElementById(guessId + "-dailyDoubleValue").innerText = valueInnerText; //assigns daily double innertext the value of the bet
    } else { //else this is a regular question
        valueInnerText = parseInt(document.getElementById(guessId + "-value").innerText.substring(1));
    }

    var cleanAnswer = answerInnerText.replace(/\[a-z]/g, ""); //remove special character designation
    
    if (check(guessValue, answerInnerText) == true) {      
        document.getElementById(guessId + "-marked").innerText = "Correct";        
        document.getElementById(guessId + "-marked").style.backgroundColor = "limegreen";
        document.getElementById(guessId + "-result").innerText = "Correct!"; //sets clue's innertext to 'correct' temporarily, hides in hideQuestion
        document.getElementById(guessId + "-result").className = "";
        document.getElementById(guessId + "-clue").className = "hidden";
        globalCorrect++;
        scoreInnerText += valueInnerText;
        document.getElementById("score").innerText = scoreInnerText;
        document.getElementById(guessId + "-answer").innerText = cleanAnswer;
        hideQuestion(guessId);
    } else {
        document.getElementById(guessId + "-marked").innerText = "Incorrect";
        document.getElementById(guessId + "-marked").style.backgroundColor = "tomato";
        document.getElementById(guessId + "-result").innerText = "Incorrect!"; //sets clue's innertext to 'incorrect' temporarily, hides in hideQuestion        
        document.getElementById(guessId + "-result").className = "";
        document.getElementById(guessId + "-clue").className = "hidden";
        globalIncorrect++;
        scoreInnerText -= valueInnerText;
        document.getElementById("score").innerText = scoreInnerText;
        document.getElementById(guessId + "-answer").innerText = cleanAnswer;
        hideQuestion(guessId);
    }
}

//does a deep dive check of whether the guess matches the answer
function check(guess, answer) {
    var removePunctuationPattern = /\w|\s/g; //pattern to be used to remove punctuation
    
    guess = guess.match(removePunctuationPattern).toString(); //remove punctuation                    
    guess = guess.replace(/,|_/g, ""); //remove commas from punctuation match result
    guess = guess.replace(/ or |the |a |an /g, "");
    guess = guess.trim(); //trim trailing and following white space

    //translate strings into string arrays
    var guessStringArray = guess.split(" ");
    var answerStringArray = answer.split(" ");

    var matchLog = [];
    var match = 0;
    for (var i = 0; i < answerStringArray.length; i++) {
        var answerTargetString = answerStringArray[i];
        for (var j = 0; j < guessStringArray.length; j++) {
            var matches = 0;

            var answerCharArray = answerTargetString.split("");
            var guessCharArray = guessStringArray[j].split("");

            var answerCharArrayReverse = answerTargetString.split("").reverse();
            var guessCharArrayReverse = guessStringArray[j].split("").reverse();
            
            var tCeiling = (answerCharArray.length >= guessCharArray.length) ? guessCharArray.length : answerCharArray.length;
            //check whether letters are matching positional forwards or backwards
            for (var t = 0; t < tCeiling; t++) {
                if (answerCharArray[t] == guessCharArray[t] || answerCharArrayReverse[t] == guessCharArrayReverse[t]) {
                    matches++;
                }
            }
            var percentage = matches / tCeiling;
            if (percentage >= .6) {
                //if match is equal to or over 50%
                matchLog.push("x");
            } else {
                //if there is no match
                matchLog.push("o");
            }
        }
        for (var m = 0; m < matchLog.length; m++) {
            if (matchLog[m] == "x") {
                match++;
            }
        }
        matchLog = [];
    }

    if (match / answerStringArray.length >= .50) {
        return true;
    } else {
        return false;
    }
}

//hides clue, guessBox, and timer. clears timeout and unfreezes click events
function hideQuestion(questionId) {
    globalTransitionCounter++; //monitors players progress, used to initiate auto transition from j to dj, after player finishes j
    if (document.getElementById(questionId + "-dailyDoubleBet") != null) {
        document.getElementById(questionId + "-dailyDoubleBet").className = "hidden";
    }
    if (document.getElementById(questionId + "-userGuess").innerText != "Unanswered") {
        setTimeout(function () { document.getElementById(questionId + "-result").className = "hidden" }, 1000);
    } else {
        document.getElementById(questionId + "-result").className = "hidden"
    }
    document.getElementById(questionId + "-guessBox").className = "hidden";
    globalTimerElement.className = "hidden";
    clearTimeout(globalY);
    globalTimerElement = null;
    globalQuestionTime = 20; 
    document.removeEventListener("click", handler, true); //removes event handler that freezes further clicks    
    globalTally = globalCorrect + "/" + globalIncorrect + "/" + globalUnanswered;
    setTimeout(transitionCheck, 1000);
}

//timer will be cancelled if checkAnswer is called before 0, if not, timer will subtract points and hide question
function timer() {
    var elementId = globalTimerElement.id.substring(0, globalTimerElement.id.indexOf("-")); //isolate questionId    
    var valueInnerText;
    globalTimerElement.innerText = globalQuestionTime; //decrements html value as timer ticks
    globalQuestionTime -= 1;
    globalY = setTimeout(timer, 1000);

    if (document.getElementById(elementId + "-value") == null) { //handling for dailydouble
        if (globalBetValue == null) { //if timer has been triggered through dailydoublevalueclick function, no need to assign value
            valueInnerText = 0;
        } else {
            valueInnerText = globalBetValue; //assign dailyDoubleBet
        }
    } else {
        valueInnerText = document.getElementById(elementId + "-value").innerText.substring(1); //assigns regular bet
    }

    if (globalQuestionTime == -1) { //if timer hits 0
        globalUnanswered++;        
        document.getElementById(elementId + "-userGuess").innerText = "Unanswered";
        hideQuestion(elementId);
        clearTimeout(globalY);
    }
}

//allows player opportunity to adjust the results of a round
function adjustment(round) {
    document.getElementById("roundReviewTitle").className = ""
    document.getElementById("reviewFinished").className = "";
    document.getElementById("reviewFinished").addEventListener("click", function () { //a checker that packages global tally and calls ajaxTransition                
        globalTally = globalCorrect + "/" + globalIncorrect + "/" + globalUnanswered;
        ajaxTransitionControllerCall(globalScoreElement.innerText.substring(globalScoreElement.innerText.indexOf("$") + 1), round, globalTally);
    });
    if (round == "FinalJeopardyRound") {
        document.getElementById("result").className = "hidden";
        document.getElementById("score").innerText = "Score: $" + globalScoreElement.innerText;
        document.getElementById("answer").innerHTML = "<b>ANSWER: </b>" + document.getElementById("answer").innerText.toUpperCase();
        document.getElementById("userGuess").innerHTML = "<b>YOUR GUESS: </b>" + document.getElementById("userGuess").innerText.toUpperCase();

        document.getElementById("clue").className = "hidden";
        document.getElementById("score").className = "";
        document.getElementById("userGuess").className = "";
        document.getElementById("answer").className = "";
        document.getElementById("marked").className = "";
    } else {
        for (var i = 0; i < 30; i++) {
            if (document.getElementById(i.toString() + "-userGuess").innerText != "Unanswered") { //checks whether question was answered
                document.getElementById(i.toString() + "-clue").innerHTML = "<b>CLUE: </b>" + document.getElementById(i.toString() + "-clue").innerText.toUpperCase();
                document.getElementById(i.toString() + "-answer").innerHTML = "<b>ANSWER: </b>" + document.getElementById(i.toString() + "-answer").innerText.toUpperCase();
                document.getElementById(i.toString() + "-userGuess").innerHTML = "<b>YOUR GUESS: </b>" + document.getElementById(i.toString() + "-userGuess").innerText.toUpperCase();

                document.getElementById(i.toString() + "-clue").className = ""; //shows answer element
                document.getElementById(i.toString() + "-answer").className = ""; //shows answer element
                document.getElementById(i.toString() + "-userGuess").className = ""; //shows userGuess element
                document.getElementById(i.toString() + "-marked").className = ""; //shows marked element

                document.getElementById(i.toString() + "-value") != null ? document.getElementById(i.toString() + "-value").className = "hidden" : ""; //handles for daily doubles
                document.getElementById(i.toString() + "-dailyDoubleValue") != null ? document.getElementById(i.toString() + "-dailyDoubleValue").className = "hidden" : ""; //handles for daily doubles                                    
            } else { //handling for unanswered
                document.getElementById(i.toString() + "-answer").innerHTML = "<b>ANSWER: </b>" + document.getElementById(i.toString() + "-answer").innerText;
                document.getElementById(i.toString() + "-userGuess").innerHTML = "<b>UNANSWERED</b>"

                document.getElementById(i.toString() + "-answer").className = ""; //shows answer element
                document.getElementById(i.toString() + "-userGuess").className = ""; //shows userGuess element

                document.getElementById(i.toString() + "-value") != null ? document.getElementById(i.toString() + "-value").className = "hidden" : ""; //handles for daily doubles
                document.getElementById(i.toString() + "-dailyDoubleValue") != null ? document.getElementById(i.toString() + "-dailyDoubleValue").className = "hidden" : ""; //handles for daily doubles
            }     
        }
    }
}

//reverses the result of a particular question
function reverse(buttonId) {
    var button = document.getElementById(buttonId);
    var isolatedId = globalPageId != "Final Jeopardy" ? buttonId.substring(0, buttonId.indexOf("-")) : buttonId;
    var value;

    if (document.getElementById(isolatedId + "-value") != null) {
        value = document.getElementById(isolatedId + "-value").innerText.substring(1);
    } else if (document.getElementById(isolatedId + "-dailyDoubleValue") != null) {
        value = document.getElementById(isolatedId + "-dailyDoubleValue").innerText
    } else {
        value = globalBetValue.toString();
    }

    var marked = globalPageId != "Final Jeopardy" ? document.getElementById(isolatedId + "-marked").innerText : document.getElementById("marked").innerText;
    if (marked == "Incorrect") { //if marked incorrect, switch to correct and adjust relevant variables        
        if (globalPageId == "Final Jeopardy") {
            globalScoreElement.innerText = "Score: $" + (parseInt(globalScoreElement.innerText.substring(globalScoreElement.innerText.indexOf("$") + 1)) + parseInt(value * 2)).toString();
        } else {
            globalScoreElement.innerText = (parseInt(globalScoreElement.innerText.substring(globalScoreElement.innerText.indexOf("$") + 1)) + parseInt(value * 2)).toString();
        }                
        globalCorrect += 1;
        globalIncorrect -= 1;
        button.style.backgroundColor = "limegreen";
        button.innerText = "Correct";        
        button.removeAttribute("onclick");
    } else if (marked == "Correct") { //if marked correct, switch to incorrect and adjust variables accordingly
        if (globalPageId == "Final Jeopardy") {
            globalScoreElement.innerText = "Score: $" + (parseInt(globalScoreElement.innerText.substring(globalScoreElement.innerText.indexOf("$") + 1)) - parseInt(value * 2)).toString();
        } else {
            globalScoreElement.innerText = (parseInt(globalScoreElement.innerText.substring(globalScoreElement.innerText.indexOf("$") + 1)) - parseInt(value * 2)).toString();
        }
        globalIncorrect += 1;
        globalCorrect -= 1;
        button.style.backgroundColor = "tomato";
        button.innerText = "Incorrect";
        button.removeAttribute("onclick");
    }    
}

//checks whether it is time for a round to be over
function transitionCheck() {    
    if (document.getElementById("clueListCount") != null) {
        globalClueListCount = parseInt(document.getElementById("clueListCount").innerText);
    }
    if (globalPageId == "Jeopardy" && globalClueListCount == globalTransitionCounter) { //checks round and transitioncounter
        adjustment("JeopardyRound");        
    } else if (globalPageId == "Double Jeopardy" && globalClueListCount == globalTransitionCounter) {
        adjustment("DoubleJeopardyRound");
    } else if (globalPageId == "Final Jeopardy") { //called from finaljeopardy handler if bet timer runs out
        adjustment("FinalJeopardyRound");
    }    
}

//ajax stringifies the data sent to it by transitionCheck() and sends it to transition post. transition post deposits 
//the data into the database and sends back a json path back to ajax. the data is parsed through success and used to 
//transition the page
function ajaxTransitionControllerCall(scoreJS, roundJS, tallyJS) {
    $.ajax({
        url: "/Game/TransitionPost" ,
        data: JSON.stringify({ score: scoreJS, round: roundJS, tally: tallyJS }), //if info doesn't pass, remove second parameter, might be set up wrong
        contentType: "application/json; charset=utf-8",
        datatype: 'json',
        type: "POST",
        success: function (data) {            
            window.location.href = data;
        },
        error: function () {
            alert("Something went wrong. Please refresh.")
        }
    });
}

//freezes all click events
function handler(e) {
    e.stopPropagation();
    e.preventDefault();
}