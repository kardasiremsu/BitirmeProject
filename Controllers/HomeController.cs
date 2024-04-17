﻿using BitirmeProj.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BitirmeProj.Data;
///using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BitirmeProj.Services;
using Microsoft.AspNetCore.Identity; // Import the necessary namespaces

namespace BitirmeProj.Controllers
{
    public class HomeController : Controller
    {
        //    private readonly ILogger<HomeController> _logger;

        private readonly ApplicationDBContext _context;
        private readonly IUserSessionService _userSessionService;

        public HomeController(ApplicationDBContext context, IUserSessionService userSessionService)
        {
            _context = context;
            _userSessionService = userSessionService;
        }
    

        public async Task<IActionResult> Index(string sortOrder, string searchTerm, int? experienceLevel, string jobLocation, int? jobType)
        {
            var jobs = from j in _context.JobListings select j;

            ViewData["TitleSortParam"] = String.IsNullOrEmpty(sortOrder) ? "titleDesc" : "";
            ViewData["DateSortParam"] = sortOrder == "date" ? "dataDesc" : "date";

            if (!string.IsNullOrEmpty(jobLocation))
            {
                jobs = jobs.Where(j => j.JobLocation == jobLocation);
            }
          
            if (!string.IsNullOrEmpty(searchTerm))
            {
                jobs = jobs.Where(j => j.JobTitle.Contains(searchTerm) || j.JobDescription.Contains(searchTerm));
            }

            if (experienceLevel != null)
            {
                jobs = jobs.Where(j => j.ExperienceLevel == experienceLevel);
            }

            if (jobType != null)
            {
                jobs = jobs.Where(j => j.JobType == jobType);
            }

            switch (sortOrder)
            {
                case "titleDesc":
                    jobs = jobs.OrderByDescending(j => j.JobTitle);
                    break;
                case "date":
                    jobs = jobs.OrderBy(j => j.JobCreatedDate);
                    break;
                case "dataDesc":
                    jobs = jobs.OrderByDescending(j => j.JobCreatedDate);
                    break;
                default:
                    jobs = jobs.OrderBy(j => j.JobTitle);
                    break;
            }

            User currentUser = _userSessionService.GetCurrentUser();

            // Pass user data to the view
            ViewBag.CurrentUser = currentUser;
            return View(await jobs.AsNoTracking().ToListAsync());
        }

        public IActionResult CreateApplication()
        {
            return View();
        }


        [HttpPost]
        public IActionResult CreateApplication(int jobID)
        {
            try
            {
                User currentUser = _userSessionService.GetCurrentUser();
                // Assuming you have the necessary logic to create the application record
                Application application = new Application();

                application.JobID = jobID;
                application.UserID = currentUser.UserID;
                application.ApplicationDate = DateTime.Now;
                application.Status = "Pending"; // Set the initial status as per your requirement
                application.ApplicationNote = "applicationNote";
                application.CoverLetter = "coverLetter";
                application.CVID = 1;

                _context.Applications.Add(application);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Application posted successfully";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Return an error response
                return StatusCode(500, $"An error occurred while creating the application: {ex.Message}");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult MyJobs()
        {
            return View();

        }
        public IActionResult MyApplications()
        {
            return View();
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}