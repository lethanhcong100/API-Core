using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Dapper;
//using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WebApplication1.Models;
using StackExchange.Redis;
using System.Text;
using WebApplication1.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace WebApplication1.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly TodoContext _context;
        private readonly AppSettings _appSettings;
        private readonly StackExchange.Redis.IDatabase _database;

        public TodoItemsController(IOptions<AppSettings> appSettings, TodoContext context, StackExchange.Redis.IDatabase database)
        {
            _context = context;
            _database = database;
            _appSettings = appSettings.Value;

        }

        public class Roles
        {
            public string Role { get; set; }
        }
        // GET: api/TodoItems

        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult GetTodoItems()
        {
            // Nhận Token rồi xét Token có trong Redis không.
            string authHeader = HttpContext.Request.Headers["Authorization"];
            string[] TokenArray = authHeader.Split(" ");
            var llen = _database.ListLength("mylist");
            var lindex = _database.ListGetByIndex("mylist", llen - 1);
            object data = new object();
            List<TodoItem> todoItem = new List<TodoItem>();
            Connect kn = new Connect();
            kn.kn_CSDL();

            // Đọc JWT rồi convert để lấy roles
            var stream = TokenArray[1];
            var handler = new JwtSecurityTokenHandler();
            var tokenS = handler.ReadToken(stream) as JwtSecurityToken;
            var jsonPayload = tokenS.Payload.SerializeToJson();

            Roles roles = new Roles();
            roles = JsonConvert.DeserializeObject<Roles>(jsonPayload);
            // Nếu Token có trong Redis
            if (lindex == TokenArray[1])
            {
                // Nếu list Redis lớn hơn 2 thì xóa 1
                if (llen >= 2)
                {
                     _database.ListLeftPop("mylist");
                }
                todoItem = kn.knoi.Query<TodoItem>("GetAll",
                           commandType: CommandType.StoredProcedure).ToList();

                // Tạo một Token mới
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                     new Claim(ClaimTypes.Role, roles.Role),
                    }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                string fullbearer = tokenHandler.WriteToken(token);
                _database.ListRightPush("mylist", fullbearer);
                // Trả về danh sách item + token mới
                data = new
                {
                    token = fullbearer,
                    todoitem = todoItem
                };
            }
            return Ok(data);
        }

        [HttpPut("{id}")]
        public IActionResult PutTodoItem(int id, TodoItem todoItem)
        {
            if (id != todoItem.Id)
            {
                return BadRequest();
            }
            Connect kn = new Connect();
            kn.kn_CSDL();
            var parameter = new DynamicParameters();
            parameter.Add("name", todoItem.Name);
            parameter.Add("xuatxu", todoItem.Xuatxu);
            parameter.Add("loaihang", todoItem.Loaihang);
            parameter.Add("id", todoItem.Id);
            kn.knoi.Execute("UpdateItem", parameter, commandType: CommandType.StoredProcedure);
            kn.knoi.Close();
            return Ok(todoItem);
        }

        [HttpPost]
        public IActionResult PostTodoItem(TodoItem todoItem)
        {
            Connect kn = new Connect();
            kn.kn_CSDL();
            var parameter = new DynamicParameters();
            parameter.Add("name", todoItem.Name);
            parameter.Add("xuatxu", todoItem.Xuatxu);
            parameter.Add("loaihang", todoItem.Loaihang);
            parameter.Add("id", todoItem.Id);
            // Kiểm tra xem ID có tồn tại chưa, nếu có thì update ko thì insert.
            kn.knoi.Execute("CheckAddUpdate", parameter, commandType: CommandType.StoredProcedure);
            kn.knoi.Close();
            return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
        }

        // DELETE: api/TodoItems/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<TodoItem>> DeleteTodoItem(int id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);
            Connect kn = new Connect();
            kn.kn_CSDL();
            kn.knoi.Execute("DeleleItem", new { Id = id },
            commandType: CommandType.StoredProcedure);
            kn.knoi.Close();
            return todoItem;
        }

        private bool TodoItemExists(long id)
        {
            return _context.TodoItems.Any(e => e.Id == id);
        }
    }
}
