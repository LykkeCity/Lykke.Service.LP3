using System.Collections.Generic;
using System.Net;
using AutoMapper;
using Lykke.Service.LP3.Client;
using Lykke.Service.LP3.Client.Models.Levels;
using Lykke.Service.LP3.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.LP3.Controllers
{
    [Route("/api/[controller]")]
    public class LevelsController : Controller, ILevelsApi
    {
        private readonly ILevelsService _levelsService;

        public LevelsController(ILevelsService levelsService)
        {
            _levelsService = levelsService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<LevelModel>), (int) HttpStatusCode.OK)]
        public IReadOnlyList<LevelModel> GetAll()
        {
            var levels = _levelsService.GetLevels();

            var models = Mapper.Map<IReadOnlyList<LevelModel>>(levels);

            return models;
        }
    }
}
