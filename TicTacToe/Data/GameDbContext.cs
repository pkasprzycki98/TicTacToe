﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicTacToe.Extensions;
using TicTacToe.Models;

namespace TicTacToe.Data
{
	public class GameDbContext : DbContext
	{
		
		public DbSet<GameInvitationModel> GameInvitationModels { get; set; }
		public DbSet<GameSessionModel> GameSessionModels { get; set; }
		public DbSet<TurnModel> TurnModels { get; set; }
		public DbSet<UserModel> UserModel { get; set; }
		public GameDbContext(DbContextOptions<GameDbContext> dbContextOptions) : base(dbContextOptions)
		{

		}
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.RemovePluralizingTableNameConvetion();
		}
 	}
}