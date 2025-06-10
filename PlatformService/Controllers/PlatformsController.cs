using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.DTOs;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        private readonly IPlatformRepo _repository;
        private readonly IMapper _mapper;
        private readonly ICommandDataClient _dataClient;
        private readonly IMessageBusClient _messageBusClient;

        public PlatformsController(IPlatformRepo repository, IMapper mapper, ICommandDataClient dataClient, IMessageBusClient messageBusClient)
        {
            _repository = repository;
            _mapper = mapper;
            _dataClient = dataClient;
            _messageBusClient = messageBusClient;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
        {
            Console.WriteLine("Getting Platforms..");

            var results = _repository.GetAllPlatforms();

            return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(results));
        }

        [Route("{id}", Name = "GetPlatformById")]
        [HttpGet]
        public ActionResult<PlatformReadDto> GetPlatformById(int id)
        {
            var platform = _repository.GetPlatformById(id);

            if (platform == null)
                return NotFound();

            return Ok(_mapper.Map<PlatformReadDto>(platform));
        }

        [HttpPost]
        public async Task<ActionResult<PlatformReadDto>> CreatePlatform([FromBody] PlatformCreateDto platformCreateDto)
        {
            var platformModel = _mapper.Map<Platform>(platformCreateDto);

            _repository.CreatePlatform(platformModel);

            _repository.SaveChanges();

            var readModel = _mapper.Map<PlatformReadDto>(platformModel);

            // send sync message
            try
            {
                await _dataClient.SendPlatformToCommand(readModel);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed Sync");
            }

            // send async message
            try
            {
                var platformPublishedDto = _mapper.Map<PlatformPublishedDto>(readModel);

                platformPublishedDto.Event = "Platform_Published";

                _messageBusClient.PublishNewPlatform(platformPublishedDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed Async {ex.Message}");
            }

            // creates at route on above get. with the new id returning the readModel
            return CreatedAtRoute(nameof(GetPlatformById), new { Id = readModel.Id }, readModel);
        }
    }
}