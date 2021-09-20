// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using A2v10.Workflow.Interfaces;
using A2v10.Workflow.WebHost.Models;

namespace A2v10.Workflow.WebHost.Controllers
{
	[ApiController]
	[Route("workflow/[action]")]
	public class WorkflowController : ControllerBase
	{
		private readonly ILogger<WorkflowController> _logger;
		private readonly IWorkflowEngine _engine;

		public WorkflowController(ILogger<WorkflowController> logger, IWorkflowEngine engine)
		{
			_logger = logger;
			_engine = engine;
		}

		[HttpPost]
		[ActionName("create")]
		[Consumes("application/json")]
		public async Task<IActionResult> Create([FromBody] CreateRequest rq)
		{
			var res = await _engine.CreateAsync(new WorkflowIdentity()
			{
				Id = rq.Workflow,
				Version = rq.Version
			});
			return Ok(new CreateResponse()
			{
				InstanceId = res.Id
			});
		}

		[HttpPost]
		[ActionName("run")]
		[Consumes("application/json")]
		public async Task<RunResponse> Run(RunRequest rq)
		{
			var res = await _engine.RunAsync(rq.InstanceId, rq.Parameters);
			return new RunResponse()
			{
				ExecutionStatus = res.ExecutionStatus.ToString(),
				Result = res.Result
			};
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
