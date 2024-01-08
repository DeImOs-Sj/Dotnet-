using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace YourNamespace
{
    public class FormData
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Contact { get; set; }

        [Required]
        public string Address { get; set; }

        [Required, RegularExpression(@"^\d{12}$")]
        public string Aadhar { get; set; }

        [Required]
        public string Dob { get; set; }
    }

    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("mongodb://localhost:27017"); 
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase("formdata");
        }

        public IMongoCollection<FormData> FormDatas => _database.GetCollection<FormData>("FormData");
    }

    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<MongoDbContext>();
            services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class FormDataController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public FormDataController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult PostFormData([FromBody] FormData formData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.FormDatas.InsertOne(formData);

            return Ok(new { success = true, message = "Form data saved successfully" });
        }

        [HttpGet]
        public IActionResult GetFormDatas()
        {
            var formData = _context.FormDatas.Find(_ => true).ToList();
            return Ok(formData);
        }

        [HttpGet("{id}")]
        public IActionResult GetFormDataById(string id)
        {
            var formData = _context.FormDatas.Find(f => f.Id == id).FirstOrDefault();

            if (formData == null)
            {
                return NotFound(new { message = "Form data not found" });
            }

            return Ok(formData);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateFormData(string id, [FromBody] FormData updatedFormData)
        {
            var filter = Builders<FormData>.Filter.Eq(f => f.Id, id);
            var update = Builders<FormData>.Update
                .Set(f => f.Name, updatedFormData.Name)
                .Set(f => f.Email, updatedFormData.Email)
                .Set(f => f.Password, updatedFormData.Password)
                .Set(f => f.Contact, updatedFormData.Contact)
                .Set(f => f.Address, updatedFormData.Address)
                .Set(f => f.Aadhar, updatedFormData.Aadhar)
                .Set(f => f.Dob, updatedFormData.Dob);

            var result = _context.FormDatas.UpdateOne(filter, update);

            if (result.ModifiedCount == 0)
            {
                return NotFound(new { success = false, message = "Form data not found" });
            }

            return Ok(new { success = true, message = "Form data updated successfully" });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteFormData(string id)
        {
            var result = _context.FormDatas.DeleteOne(f => f.Id == id);

            if (result.DeletedCount == 0)
            {
                return NotFound(new { success = false, message = "Form data not found" });
            }

            return Ok(new { success = true, message = "Form data deleted successfully" });
        }
    }
}
