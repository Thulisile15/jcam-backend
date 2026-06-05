using Microsoft.AspNetCore.Mvc;
using JCAM_CONNECT.Models;
using JCAM_CONNECT.Repositories;

namespace JCAM_CONNECT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly ILogger<EventsController> _logger;
        private readonly IRepository<Event> _eventsRepository;

        public EventsController(ILogger<EventsController> logger, IRepository<Event> eventsRepository)
        {
            _logger = logger;
            _eventsRepository = eventsRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            try
            {
                var events = await _eventsRepository.GetAllAsync();
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events");
                return StatusCode(500, new { message = "An error occurred while fetching events" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(int id)
        {
            var eventItem = await _eventsRepository.GetByIdAsync(id);
            if (eventItem == null)
                return NotFound(new { message = "Event not found" });

            return Ok(eventItem);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] Event eventData)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Set default values for new event
                eventData.CreatedAt = DateTime.UtcNow;
                eventData.IsActive = true;

                var createdEvent = await _eventsRepository.AddAsync(eventData);
                return CreatedAtAction(nameof(GetEventById), new { id = createdEvent.Id }, createdEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event");
                return StatusCode(500, new { message = "An error occurred while creating the event" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] Event eventData)
        {
            var existingEvent = await _eventsRepository.GetByIdAsync(id);
            if (existingEvent == null)
                return NotFound(new { message = "Event not found" });

            // Update properties
            existingEvent.Title = eventData.Title;
            existingEvent.Description = eventData.Description;
            existingEvent.EventDate = eventData.EventDate;
            existingEvent.Location = eventData.Location;
            existingEvent.StartTime = eventData.StartTime;
            existingEvent.EndTime = eventData.EndTime;
            existingEvent.EventType = eventData.EventType;
            existingEvent.IsActive = eventData.IsActive;

            await _eventsRepository.UpdateAsync(existingEvent);

            return Ok(existingEvent);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventItem = await _eventsRepository.GetByIdAsync(id);
            if (eventItem == null)
                return NotFound(new { message = "Event not found" });

            await _eventsRepository.DeleteAsync(eventItem);

            return Ok(new { message = "Event deleted successfully" });
        }
    }
}