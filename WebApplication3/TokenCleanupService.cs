namespace WebApplication3
{
    public class TokenCleanupService
    {
        private readonly ApplicationDbContext _context;

        public TokenCleanupService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void RemoveExpiredTokens()
        {
            var currentDate = DateTime.UtcNow; // используем UTC

            var tokensToRemove = _context.InvalidTokens
                .Where(token => token.ExpiryDate < currentDate)
                .ToList();

            if (tokensToRemove.Any())
            {
                _context.InvalidTokens.RemoveRange(tokensToRemove);
                _context.SaveChanges();
            }
        }

    }
}
