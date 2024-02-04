using Gubon.Gateway.Store.FreeSql.Management;
using Gubon.Gateway.Store;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Gubon.Gateway.Store.FreeSql.Models;
using Gubon.Gateway.Authorization;
using FreeRedis;
using Gubon.Gateway.Utils.Config;
using Gubon.Gateway.Store.FreeSql.Models.Dto;
using Gubon.Gateway.IResult;

namespace Gubon.Gateway.Controllers
{
    [Route("__admin/api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<ReverseProxyController> _logger;
        private readonly IFreeSql _ifreeSql;
        private readonly IConfiguration _configuration;
        private readonly IJwtUtils _jwtUtils;
        private readonly IRedisClient _redisClient;
        private readonly GubonSettings _gubonsetting;


        public UserController(ILogger<ReverseProxyController> logger, IConfiguration configuration, IFreeSql ifreeSql, IJwtUtils jwtUtils, IRedisClient redisClient, GubonSettings gubonSettings)
        {
            _logger = logger;
            _configuration = configuration;
            _ifreeSql = ifreeSql;
            _jwtUtils = jwtUtils;
            _redisClient = redisClient;
            _gubonsetting = gubonSettings;
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login(AccountLogin accountLogin)
        {
            var ret = new JResult() { code = 0, data = null, total = 0, status = 1, error = string.Empty };
            var user = await _ifreeSql.GetRepository<User>().Where(u => u.Account == accountLogin.Account && u.Password == accountLogin.Password && u.Status).FirstAsync();
            if (user != null && !string.IsNullOrWhiteSpace(user.Account))
            {
                ret.data =  new { username = user.UserName, token = _jwtUtils.GenerateJwtToken(user), expired = _gubonsetting.JwtSettings.ExpiredTime };
            }
            else
            {
                ret.code = 1;
                ret.error = "请检查账号密码";
            }
            return Ok(ret);
        }


    }
}
