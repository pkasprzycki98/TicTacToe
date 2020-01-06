﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TicTacToe.Models;
using TicTacToe.Services;
using Xunit;

namespace TicTacToe.UnitTests
{
	public class UserServiceTests
	{
		[Theory]
		[InlineData("test@test.com","test","test","test123!")]
		[InlineData("test1@test.com","test","test","test123!")]
		[InlineData("test2@test.com","test","test","test123!")]
		[InlineData("test3@test.com","test","test","test123!")]
		public async Task ShouldAddUser(string email, string firstName, string lastName, string password)
		{
			var userModel = new UserModel
			{
				Email = email,
				FirstName = firstName,
				LastName = lastName,
				Password = password
			};
			var userService = new UserService();
			var userAdded = await userService.RegisterUser(userModel);
			Assert.True(userAdded);
		}
	}
}