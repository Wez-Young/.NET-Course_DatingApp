using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    public class BugController : BaseAPIController
    {
        private readonly DataContext _context;

        public BugController(DataContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("auth")]
        public ActionResult<string> GetSecret()
        {
            return "secret text";
        }

        [HttpGet("not-found")]
        public ActionResult<AppUser> GetNotFound()
        {
            var result = _context.Users.Find(-1);

            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpGet("server-error")]
        public ActionResult<string> GetServerError()
        {
            var result = _context.Users.Find(-1);

            return result.ToString();

        }

        [HttpGet("bad-request")]
        public ActionResult<string> GetBadRequest()
        {
            return BadRequest("Request went bad");
        }
    }
}