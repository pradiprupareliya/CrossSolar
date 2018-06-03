﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrossSolar.Domain;
using CrossSolar.Models;
using CrossSolar.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrossSolar.Controllers
{
    [Route("panel")]
    public class AnalyticsController : Controller
    {
        private readonly IAnalyticsRepository _analyticsRepository;

        private readonly IPanelRepository _panelRepository;

        public AnalyticsController(IAnalyticsRepository analyticsRepository, IPanelRepository panelRepository)
        {
            _analyticsRepository = analyticsRepository;
            _panelRepository = panelRepository;
        }

        // GET panel/XXXX1111YYYY2222/analytics
        [HttpGet("{panelId}/[controller]")]
        public async Task<IActionResult> Get([FromRoute]string panelId)
        {
            var panel = await _panelRepository.Query()
                .FirstOrDefaultAsync(x => x.Serial.Equals(panelId, StringComparison.CurrentCultureIgnoreCase));

            if (panel == null)
            {
                return NotFound();
            }

            var analytics = await _analyticsRepository.Query()
                .Where(x => x.PanelId.Equals(panelId, StringComparison.CurrentCultureIgnoreCase)).ToListAsync();

            var result = new OneHourElectricityListModel
            {
                OneHourElectricitys = analytics.Select(c => new OneHourElectricityModel
                {
                    Id = c.Id,
                    KiloWatt = c.KiloWatt,
                    DateTime = c.DateTime
                })
            };

            return Ok(result);
        }

        // GET panel/XXXX1111YYYY2222/analytics/day
        [HttpGet("{panelId}/[controller]/day")]
        public async Task<IActionResult> DayResults([FromRoute]string panelId)
        {
            //var result = new List<OneDayElectricityModel>();
            if(string.IsNullOrEmpty(panelId))
            {
                // For all
                var result = await _analyticsRepository.Query().Where(x => x.DateTime.Date < DateTime.Now.Date).GroupBy(x => x.DateTime.Date).Select(a => new
                {
                    Sum = a.Sum(x => x.KiloWatt),
                    Average = a.Average(x => x.KiloWatt),
                    Maximum = a.Max(x => x.KiloWatt),
                    Minimum = a.Min(x => x.KiloWatt),
                    DateTime = a.Key
                }).ToListAsync();

                return Ok(result);
            }
            else
            {
                // Only for particular panelId.
                var result = await _analyticsRepository.Query().Where(x => x.PanelId == panelId && x.DateTime.Date < DateTime.Now.Date).GroupBy(x => x.DateTime.Date).Select(a => new
                {
                    Sum = a.Sum(x => x.KiloWatt),
                    Average = a.Average(x => x.KiloWatt),
                    Maximum = a.Max(x => x.KiloWatt),
                    Minimum = a.Min(x => x.KiloWatt),
                    DateTime = a.Key
                }).ToListAsync();

                return Ok(result);
            }
        }

        // POST panel/XXXX1111YYYY2222/analytics
        [HttpPost("{panelId}/[controller]")]
        public async Task<IActionResult> Post([FromRoute]string panelId, [FromBody]OneHourElectricityModel value)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var oneHourElectricityContent = new OneHourElectricity
            {
                PanelId = panelId,
                KiloWatt = value.KiloWatt,
                DateTime = DateTime.UtcNow
            };

            await _analyticsRepository.InsertAsync(oneHourElectricityContent);

            var result = new OneHourElectricityModel
            {
                Id = oneHourElectricityContent.Id,
                KiloWatt = oneHourElectricityContent.KiloWatt,
                DateTime = oneHourElectricityContent.DateTime
            };

            return Created($"panel/{panelId}/analytics/{result.Id}", result);
        }
    }
}
