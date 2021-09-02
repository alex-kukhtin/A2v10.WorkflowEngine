using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace A2v10.Workflow.WebHost.Controllers
{
	[ApiController]
	[Route("workflow/[action]")]
	public class WorkflowController : ControllerBase
	{
		private static readonly string[] Summaries = new[]
		{
			"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
		};

		private readonly ILogger<WorkflowController> _logger;

		public WorkflowController(ILogger<WorkflowController> logger)
		{
			_logger = logger;
		}

		[HttpPost]
		[ActionName("create")]
		[Consumes("application/json")]
		public ActionResult Create()
		{
			return Ok(0);
		}

		[HttpPost]
		[ActionName("start")]
		[Consumes("application/json")]
		public ActionResult Start()
		{
			return Ok(1);
		}

		[HttpPost]
		[ActionName("resume")]
		[Consumes("application/json")]
		public ActionResult Resume()
		{
			return Ok(2);
		}
	}
}
