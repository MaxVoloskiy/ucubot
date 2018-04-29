using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using ucubot.Model;

namespace ucubot.Controllers
{
    [Route("api/[controller]")]
    public class LessonSignalEndpointController : Controller
    {
        private readonly IConfiguration _configuration;

        public LessonSignalEndpointController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IEnumerable<LessonSignalDto> ShowSignals()
        {
            var connection = new MySqlConnection(_configuration.GetConnectionString("BotDatabase"));
            
            connection.Open();
            var adapter = new MySqlDataAdapter("SELECT * FROM lesson_signal", connection);
            
            var dataset = new DataSet();
            
            adapter.Fill(dataset, "lesson_signal");

            foreach (DataRow row in dataset.Tables[0].Rows)
            {
                var lessonsignaldto = new LessonSignalDto
                {
                    Id = (int) row["id"],
                    Timestamp = (DateTime) row["time_stamp"],
                    Type = (LessonSignalType) row["signal_type"],
                    UserId = row["user_id"].ToString()
                };
                yield return lessonsignaldto;
            }
            
        }
        
        [HttpGet("{id}")]
        public LessonSignalDto ShowSignal(long id)
        {
            var connection = new MySqlConnection(_configuration.GetConnectionString("BotDatabase"));
            
            connection.Open();
            var command = new MySqlCommand("SELECT * FROM lesson_signal WHERE id = " + id, connection);
            command.Parameters.AddWithValue("id", id);
            var adapter = new MySqlDataAdapter(command);
            
            var dataset = new DataSet();
            
            adapter.Fill(dataset, "lesson_signal");
            if (dataset.Tables[0].Rows.Count < 1)
                return null;
            
            var row = dataset.Tables[0].Rows[0];
            var lessonSignalDto = new LessonSignalDto
            {
                Timestamp = (DateTime) row["time_stamp"],
                Type = (LessonSignalType) row["signal_type"],
                UserId = row["user_id"].ToString()
            };
            return lessonSignalDto;
            
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateSignal(SlackMessage message)
        {
            var userId = message.user_id;
            var signalType = message.text.ConvertSlackMessageToSignalType();

            var connection = new MySqlConnection(_configuration.GetConnectionString("BotDatabase"));
            
            connection.Open();
            var command = connection.CreateCommand();
            
            
            
            MySqlCommand check_User_Id = new MySqlCommand("SELECT COUNT(*) FROM lesson_signal WHERE (student_id = @userId)" , connection);
            check_User_Id.Parameters.AddWithValue("@userId", userId);
            int UserExist = (int)check_User_Id.ExecuteScalar();

            if(UserExist > 0)
            {
                //Username exist
                command.CommandText =
                    "INSERT INTO lesson_signal (user_id, signal_type) VALUES (@userId, @signalType);";
                command.Parameters.AddRange(new[]
                {
                    new MySqlParameter("userId", userId),
                    new MySqlParameter("signalType", signalType)
                });
            }
            else
            {
                //Username doesn't exist.
                return BadRequest();
            }
            
            await command.ExecuteNonQueryAsync();
            
            return Accepted();
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveSignal(long id)
        {
            var conn = new MySqlConnection(_configuration.GetConnectionString("BotDatabase"));
            
            conn.Open();
            var command = conn.CreateCommand();
            command.CommandText =
                "DELETE FROM lesson_signal WHERE ID = " + id + ";";
            await command.ExecuteNonQueryAsync();
            
            return Accepted();
        }
    }
}
