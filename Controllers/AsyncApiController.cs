using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiAyncApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

namespace ApiAyncApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AsyncApiController : ControllerBase
    {
        private static Dictionary<Guid, bool> runningTasks = new Dictionary<Guid, bool>();
        private IWebHostEnvironment environment;
        public AsyncApiController(IWebHostEnvironment _environment)
        {
            this.environment = _environment;
        }
        /// <summary>
        /// This is the method that starts the task running.  It creates a new thread to complete the work on, and returns an ID which can be passed in to check the status of the job.  
        /// In a real world scenario your dictionary may contain the object you want to return when the work is done.
        /// </summary>
        /// <returns>HTTP Response with needed headers</returns>
        [HttpGet]
        [Route("/api/startwork")]
        public async Task<Response> longrunningtask()
        {
            Guid id = Guid.NewGuid();  //Generate tracking Id
            runningTasks[id] = false;  //Job isn't done yet
            new Thread(() => doWork(id)).Start();   //Start the thread of work, but continue on before it completes
            Response response = new Response();
            response.isCompleted = false;
            response.isError = false;
            response.id = id;
            response.url = "api/status";
            /*HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.Accepted);
            //HttpResponseMessage responseMessage = Request.CreateResponse(HttpStatusCode.Accepted);
            responseMessage.Headers.Add("location", String.Format("{0}://{1}/api/status/{2}", Request.RequestUri.Scheme, Request.RequestUri.Host, id));  //Where the engine will poll to check status
            responseMessage.Headers.Add("retry-after", "20");   //How many seconds it should wait (20 is default if not included)
            return responseMessage;*/
            return response;
        }


        /// <summary>
        /// This is where the actual long running work would occur.
        /// </summary>
        /// <param name="id"></param>
        private void doWork(Guid id)
        {
            Debug.WriteLine("Starting work");
            Task.Delay(120000).Wait(); //Do work will work for 120 seconds)
            Debug.WriteLine("Work completed");
            runningTasks[id] = true;  //Set the flag to true - work done
        }

        /// <summary>
        /// Method to check the status of the job.  This is where the location header redirects to.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/api/status/{id}")]
        public Response checkStatus(Guid id)
        {
            Response response = new Response();

            //If the job is complete
            if (runningTasks.ContainsKey(id) && runningTasks[id])
            {
                runningTasks.Remove(id);
                response.isCompleted = true;
                response.message = "";
                response.isError = false;
                response.url = "Template\\PPT\\ExportedReportPPT" + "filename.pptx";
                return response;
                //return Request.CreateResponse(HttpStatusCode.OK, "Some data could be returned here");
            }
            //If the job is still running
            else if (runningTasks.ContainsKey(id))
            {
                response.isCompleted = false;
                response.message = "Completed";
                response.url = ""; ;
                response.isError = false;
                return response;
                /*HttpResponseMessage responseMessage = Request.CreateResponse(HttpStatusCode.Accepted);
                responseMessage.Headers.Add("location", String.Format("{0}://{1}/api/status/{2}", Request.RequestUri.Scheme, Request.RequestUri.Host, id));  //Where the engine will poll to check status
                responseMessage.Headers.Add("retry-after", "20");
                return responseMessage;*/
            }
            else
            {
                response.isCompleted = true;
                response.message = "";
                response.url = "Template\\PPT\\ExportedReportPPT" + "filename.pptx";
                response.isError = false;
                return response;
                //return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No job exists with the specified ID");
            }
        }
        [HttpGet]
        [Route("/api/download")]
        public FileResult downloadReport(string path)
        {
      
            string p = System.IO.Path.Combine(this.environment.WebRootPath, path);
            var fs = System.IO.File.OpenRead(p);
            return File(fs, "application/vnd.openxmlformats-officedocument.presentationml.presentation", "Briefing Book" + ".pptx");
        }
    }
}