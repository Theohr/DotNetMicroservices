using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using Rebex.Net;
using System.Net;
using System;
using CommandsService.Data;
using AutoMapper;
using CommandsService.Dtos;

namespace CommandsServices.Controllers
{
    [Route("api/c/platforms")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        private readonly ICommandRepo _repo;
        private readonly IMapper _mapper;

        public PlatformsController(ICommandRepo repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
        {
            Console.WriteLine("Getting platforms from CommandService...");

            var platforms = _repo.GetAllPlatforms();

            return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(platforms)); 
        } 

        [HttpPost]
        public ActionResult TestInboundConnection()
        {
            Console.WriteLine("Inbound POST #Command Service");

            return Ok("Inbound Test OKAY");
        }

        // [HttpGet]
        // public async void GetDocumentInfo()
        // {
        //     var client = new HttpClient();

        //     var request = new HttpRequestMessage(HttpMethod.Get, "https://trueport101-accp.apps.eu-1c.mendixcloud.com/rest/documents/v1/documents/10133099162287855");

        //     request.Headers.Add("api-key", "Ch6mBEGk7lMk7nk1Hj9hw6zxbvLSAn6s9gARkJtv44Kpm4pp8DwGHArzlDiWFmLg");

        //     var response = await client.SendAsync(request);

        //     response.EnsureSuccessStatusCode();

        //     var responseContent = await response.Content.ReadAsStringAsync();

        //     JObject jsonResponse = JObject.Parse(responseContent);

        //     string fileName = jsonResponse["fileName"].ToString();
        //     string base64Binary = jsonResponse["binary"].ToString();

        //     byte[] fileBytes = Convert.FromBase64String(base64Binary);

        //     string savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

        //     File.WriteAllBytes(savePath, fileBytes);

        //     // return Ok();
        // }

        //[HttpGet]
        //public async void GetDocumentInfo()
        //{
        //    var creator = new HttpRequestCreator();

        //    var request = creator.Create("https://trueport101-accp.apps.eu-1c.mendixcloud.com/rest/documents/v1/documents/10133099162287855");

        //    request.Method = "GET";

        //    request.Headers.Add("api-key", "Ch6mBEGk7lMk7nk1Hj9hw6zxbvLSAn6s9gARkJtv44Kpm4pp8DwGHArzlDiWFmLg");

        //    using (var response = request.GetResponse())
        //    {
        //        using (var stream = response.GetResponseStream())
        //        using (var reader = new StreamReader(stream))
        //        {
        //            var responseContent = reader.ReadToEnd();
        //        }
        //    }
        //}
    }
}