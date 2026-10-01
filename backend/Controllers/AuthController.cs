using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db, IConfiguration configuration) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<KorisnikOdgovor>> Login(LoginZahtjev zahtjev)
    {
        var email = zahtjev.Email.Trim().ToLowerInvariant();
        var korisnik = await db.Korisnici.SingleOrDefaultAsync(x => x.Email == email);

        if (korisnik is null || !BCrypt.Net.BCrypt.Verify(zahtjev.Lozinka, korisnik.LozinkaHash))
            return Unauthorized(new { poruka = "Neispravan email ili lozinka." });

        var token = IzradiToken(korisnik);
        return Ok(new KorisnikOdgovor(korisnik.Id, korisnik.Ime, korisnik.Prezime,
            korisnik.Email, korisnik.Uloga, korisnik.MoraPromijenitiLozinku, token));
    }

    [Authorize]
    [HttpPost("promijeni-lozinku")]
    public async Task<IActionResult> PromijeniLozinku(PromijeniLozinkuZahtjev zahtjev)
    {
        var korisnikId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var korisnik = await db.Korisnici.SingleOrDefaultAsync(x => x.Id == korisnikId);
        if (korisnik is null) return NotFound(new { poruka = "Korisnik nije pronađen." });

        if (string.IsNullOrWhiteSpace(zahtjev.TrenutnaLozinka))
            return BadRequest(new { poruka = "Trenutna lozinka je obavezna." });

        if (!BCrypt.Net.BCrypt.Verify(zahtjev.TrenutnaLozinka, korisnik.LozinkaHash))
            return BadRequest(new { poruka = "Trenutna lozinka nije ispravna." });

        if (string.IsNullOrWhiteSpace(zahtjev.NovaLozinka) || zahtjev.NovaLozinka.Length < 6)
            return BadRequest(new { poruka = "Nova lozinka mora imati najmanje 6 znakova." });

        if (zahtjev.TrenutnaLozinka == zahtjev.NovaLozinka)
            return BadRequest(new { poruka = "Nova lozinka mora biti različita od trenutne." });

        korisnik.LozinkaHash = BCrypt.Net.BCrypt.HashPassword(zahtjev.NovaLozinka);
        korisnik.MoraPromijenitiLozinku = false;
        await db.SaveChangesAsync();
        return Ok(new { poruka = "Lozinka je uspješno promijenjena." });
    }

    private string IzradiToken(Korisnik korisnik)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, korisnik.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, korisnik.Id.ToString()),
            new Claim(ClaimTypes.Email, korisnik.Email),
            new Claim(ClaimTypes.Role, korisnik.Uloga)
        };
        var minutes = configuration.GetValue("Jwt:ExpirationMinutes", 480);
        var token = new JwtSecurityToken(configuration["Jwt:Issuer"], configuration["Jwt:Audience"],
            claims, expires: DateTime.UtcNow.AddMinutes(minutes), signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}