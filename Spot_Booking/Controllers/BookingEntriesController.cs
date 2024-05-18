using Spot_Booking.DTO;
using Spot_Booking.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;

namespace MasterDetail_Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BookingEntriesController : ControllerBase
{
    private readonly BookingDbContext _context;
    private readonly IWebHostEnvironment _env;

    public BookingEntriesController(BookingDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        this._env = env;
    }

    [HttpGet]
    [Route("GetSpots")]
    public async Task<ActionResult<IEnumerable<Spot>>> GetSpots()
    {
        return await _context.Spots.ToListAsync();
    }

    [HttpGet]
    [Route("GetClients")]
    public async Task<ActionResult<IEnumerable<Client>>> GetClients()
    {
        return await _context.Clients.Include(x => x.bookingEntries).ThenInclude(b => b.Spot).ToListAsync();
    }

    [HttpGet]
    [Route("GetClientById/{clientId}")]
    public async Task<ActionResult<Client>> GetClientById([FromRoute] int clientId)
    {
        return await _context.Clients
            .Where(x => x.ClientId == clientId)
            .Include(x => x.bookingEntries).ThenInclude(b => b.Spot)
            .FirstAsync();
    }

    // POST: api/BookingEntries
    [HttpPost]
    public async Task<ActionResult<int>> PostBookingEntry([FromForm] ClientDTO clientDTO)
    {
        int result = 0;
        Client client = new Client
        {
            ClientName = clientDTO.ClientName,
            BirthDate = clientDTO.BirthDate,
            PhoneNo = clientDTO.PhoneNo,
            MaritalStatus = clientDTO.MaritalStatus
        };

        if (clientDTO.PictureFile != null)
        {
            var webroot = _env.WebRootPath;
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(clientDTO.PictureFile.FileName);
            var filePath = Path.Combine(webroot, "Images", fileName);

            FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await clientDTO.PictureFile.CopyToAsync(fileStream);
            await fileStream.FlushAsync();
            fileStream.Close();
            client.Picture = fileName;
        }

        foreach (var item in clientDTO.SpotId)
        {
            var bookingEntry = new BookingEntry
            {
                Client = client,
                ClientId = client.ClientId,
                SpotId = item
            };
            _context.Add(bookingEntry);
        }

        result = await _context.SaveChangesAsync();

        return result;
    }

    // Update: api/BookingEntries
    [HttpPut]
    public async Task<ActionResult<int>> UpdateBookingEntry([FromForm] ClientDTO clientDTO)
    {
        int result = 0;
        Client client = new Client
        {
            ClientId = clientDTO.ClientId,
            ClientName = clientDTO.ClientName,
            BirthDate = clientDTO.BirthDate,
            PhoneNo = clientDTO.PhoneNo,
            MaritalStatus = clientDTO.MaritalStatus
        };

        if (clientDTO.PictureFile != null)
        {
            var webroot = _env.WebRootPath;
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(clientDTO.PictureFile.FileName);
            var filePath = Path.Combine(webroot, "Images", fileName);

            FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await clientDTO.PictureFile.CopyToAsync(fileStream);
            await fileStream.FlushAsync();
            fileStream.Close();
            client.Picture = fileName;
        }

        _context.Clients.Update(client);

        var existingSpots = await _context.BookingEntries
            .Where(x => x.ClientId == client.ClientId).ToListAsync();

        if (existingSpots.Any())
        {
            _context.BookingEntries.RemoveRange(existingSpots);
        }

        foreach (var item in clientDTO.SpotId)
        {
            var bookingEntry = new BookingEntry
            {
                Client = client,
                ClientId = client.ClientId,
                SpotId = item
            };
            _context.Add(bookingEntry);
        }

        result = await _context.SaveChangesAsync();

        return result;
    }



    //Delete Booking
    [Route("Delete/{id}")]
    [HttpDelete]
    public async Task<ActionResult<int>> DeleteBookingEntry(int id)
    {
        int result = 0;
        Client client = _context.Clients.Find(id);

        var existingSpots = _context.BookingEntries.Where(x => x.ClientId == client.ClientId).ToList();
        foreach (var item in existingSpots)
        {
            _context.BookingEntries.Remove(item);
        }

        _context.Entry(client).State = EntityState.Deleted;

        result = await _context.SaveChangesAsync();

        return result;
    }


}
