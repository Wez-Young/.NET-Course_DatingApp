using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using api.Data;
using api.DTOs;
using api.Entities;
using api.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    public class AccountController : BaseAPIController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        public AccountController(DataContext context, ITokenService tokenService)
        {
            _tokenService = tokenService;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AppUser>> Register(RegisterDto registerDto)
        {
            //Check if database already contains inputted username
            if (await UserExists(registerDto.Username)) return BadRequest("Username already exists!");

            //initialise hashing algorithm
            using var hmac = new HMACSHA512();
            //initialise new user based on recieved information
            var user = new AppUser
            {
                //set username to lowercase for consistency
                UserName = registerDto.Username.ToLower(),
                //Encrypt the inputted password
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key,
            };

            //Add user to the database
            _context.Users.Add(user);
            //Save the changes to the database
            await _context.SaveChangesAsync();

            return user;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(foundUser => foundUser.UserName == loginDto.Username);

            if (user == null) return Unauthorized("Invalid username!");

            //set the specified key data to the algorithm
            using var hmac = new HMACSHA512(user.PasswordSalt);
            //Encrypt the inputted password with the algorithm
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            //Check each character in inputted encrypted password and the stored one are the same
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password!");
            }

            //return username and generated token if successful
            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
            };
        }

        //Check if the inputted username is unique
        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(user => user.UserName == username.ToLower());
        }
    }
}