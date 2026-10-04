using System.Security.Claims;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Middleware;

public class MoraPromijenitiLozinkuMiddleware(RequestDelegate next)
{
    private static readonly PathString LoginPath = new("/api/auth/login");
    private static readonly PathString PromjenaLozinkePath = new("/api/auth/promijeni-lozinku");

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        var path = context.Request.Path;
        if (path.StartsWithSegments(LoginPath) || path.StartsWithSegments(PromjenaLozinkePath))
        {
            await next(context);
            return;
        }

        var idClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idClaim, out var korisnikId))
        {
            await next(context);
            return;
        }

        var korisnik = await db.Korisnici.AsNoTracking()
            .Where(x => x.Id == korisnikId)
            .Select(x => new { x.Uloga, x.MoraPromijenitiLozinku })
            .SingleOrDefaultAsync();

        if (korisnik is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var ulogaTokena = context.User.FindFirstValue(ClaimTypes.Role);
        if (!string.Equals(ulogaTokena, korisnik.Uloga, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                poruka = "Korisnička uloga je promijenjena. Prijavite se ponovno."
            });
            return;
        }

        if (korisnik.MoraPromijenitiLozinku)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                poruka = "Morate promijeniti početnu lozinku prije korištenja sustava.",
                moraPromijenitiLozinku = true
            });
            return;
        }

        await next(context);
    }
}
