using System;
using System.Threading.Tasks;
using CambioAPI.DTOs;
using CambioAPI.Services;
using CambioAPI.Models;
using Microsoft.EntityFrameworkCore;
using CambioAPI.Data;
using BCrypt.Net;

namespace CambioAPI.Data.Seeders
{
    public class UserDataSeeder
    {
        private readonly ApplicationDbContext _context;

        public UserDataSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (!await _context.Users.AnyAsync())
            {
                var masterUser = new User
                {
                    Name = "Administrador",
                    Email = "admin@cambioapi.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    IsMaster = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Users.AddAsync(masterUser);
                await _context.SaveChangesAsync();
            }
        }
    }
} 