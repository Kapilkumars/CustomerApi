using CustomerCustomerApi.Extensions;
using CustomerCustomerApi.Interfaces;
using CustomerCustomerApi.Models;
using Microsoft.Azure.CosmosRepository;

namespace CustomerCustomerApi.Services
{
    public class AuthToUserProvider : IAuthToUserProvider
    {
        private string _graphUserId;
        private string _userId;

        readonly IHttpContextAccessor _contextAccessor;
        private readonly IRepository<UserCosmosDb> _userRepository;

        public AuthToUserProvider(IHttpContextAccessor contextAccessor, IRepository<UserCosmosDb> userRepository)
        {
            _contextAccessor = contextAccessor;
            _userRepository = userRepository;
        }

        public string GraphUserId
        {
            get
            {
                if (_graphUserId != null)
                    return _graphUserId;

                _graphUserId = _contextAccessor.HttpContext!.User.GetIdentityId();
                return _graphUserId;
            }
        }

        public async Task<string> GetUserIdAsync()
        {
            if (_userId == null)
            {
                var users = await _userRepository.GetAsync(x => x.GraphUserId == GraphUserId);

                var user = users.FirstOrDefault();
                if (user == null)
                {
                    throw new InvalidDataException("User not found.");
                }

                _userId = user.Id;
            }

            return _userId;
        }
    }
}
