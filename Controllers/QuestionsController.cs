﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Asker.Models;
using Asker.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Asker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly IQuestionRepo _questionRepository;
        private readonly IAnswerRepo _answerRepository;

        public QuestionsController(IQuestionRepo questionRepository, IAnswerRepo answerRepository)
        {
            _questionRepository = questionRepository;
            _answerRepository = answerRepository;
        }

        [HttpGet]
        public async Task<IEnumerable<Question>> GetQuestions(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return await _questionRepository.GetQuestions();
            }
            return await _questionRepository.GetQuestionsBySearch(search);
        }

        [HttpGet("unanswered")]
        public async Task<IEnumerable<Question>> GetUnansweredQuestions()
        {
            return await _questionRepository.GetUnansweredQuestions();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Question>> GetQuestion(Guid id)
        {
            var question = await _questionRepository.GetQuestion(id);

            if (question == null)
            {
                return NotFound();
            }

            return question;
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Question>> PutQuestion(Guid id, QuestionPutRequest questionPutRequest)
        {
            var question = await _questionRepository.GetQuestion(id);
            if (question == null)
            {
                return NotFound();
            }
            question.Title = string.IsNullOrEmpty(questionPutRequest.Title) ? question.Title : questionPutRequest.Title;
            question.Content = string.IsNullOrEmpty(questionPutRequest.Content) ? question.Content : questionPutRequest.Content;

            return await _questionRepository.PutQuestion(id, question);
        }

        [HttpPost]
        public async Task<ActionResult<Question>> PostQuestion(QuestionPostRequest questionPostRequest)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var savedQuestion = await _questionRepository.PostQuestion(new Question
            {
                Title = questionPostRequest.Title,
                Content = questionPostRequest.Content,
                UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value,
                UserName = "admin",
                Created = DateTime.UtcNow
            });
            return CreatedAtAction("GetQuestion", new { id = savedQuestion.QuestionId }, savedQuestion);

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(Guid id)
        {
            var question = await _questionRepository.GetQuestion(id);
            if (question == null)
            {
                return NotFound();
            }

            await _questionRepository.DeleteQuestion(question);

            return NoContent();
        }

        [Authorize]
        [HttpPost("answer")]
        public async Task<ActionResult<Answer>> PostAnswer(AnswerPostRequest answerPostRequest)
        {
            var question = _questionRepository.GetQuestion(answerPostRequest.QuestionId);
            if (question == null)
            {
                return NotFound();
            }
            var savedAnswer = await _answerRepository.PostAnswer(new Answer
            {
                QuestionId = answerPostRequest.QuestionId,
                Content = answerPostRequest.Content,
                UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value,
                UserName = "admin",
                Created = DateTime.UtcNow
            });

            return savedAnswer;

        }

    }
}
