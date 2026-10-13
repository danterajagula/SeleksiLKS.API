using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Inventory.API.Models;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly string _connectionString;

    public ItemsController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var list = new List<ItemResponseDto>();
        using var conn = new SqlConnection(_connectionString);
        const string query = "SELECT id, itemCode, itemName, category, price, stock FROM dbo.items ORDER BY id ASC";
        using var cmd = new SqlCommand(query, conn);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new ItemResponseDto
            {
                Id = reader.GetInt32(0),
                ItemCode = reader.GetString(1),
                ItemName = reader.GetString(2),
                Category = reader.GetString(3),
                Price = reader.GetDecimal(4),
                Stock = reader.GetInt32(5)
            });
        }
        return Ok(list); 
    }


    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        const string query = "SELECT id, itemCode, itemName, category, price, stock FROM dbo.items WHERE id = @id";
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
        conn.Open();
        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return NotFound(new { message = $"Barang dengan ID {id} tidak ditemukan." }); // 404

        var item = new ItemResponseDto
        {
            Id = reader.GetInt32(0),
            ItemCode = reader.GetString(1),
            ItemName = reader.GetString(2),
            Category = reader.GetString(3),
            Price = reader.GetDecimal(4),
            Stock = reader.GetInt32(5)
        };
        return Ok(item); 
    }

    [HttpPost]
    public IActionResult Create([FromBody] ItemRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState); 

        try
        {
            using var conn = new SqlConnection(_connectionString);
            const string query = @"
                INSERT INTO dbo.items (itemCode, itemName, category, price, stock)
                OUTPUT INSERTED.id
                VALUES (@itemCode, @itemName, @category, @price, @stock)";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@itemCode", SqlDbType.VarChar, 50).Value = request.ItemCode;
            cmd.Parameters.Add("@itemName", SqlDbType.NVarChar, 150).Value = request.ItemName;
            cmd.Parameters.Add("@category", SqlDbType.NVarChar, 50).Value = request.Category;
            cmd.Parameters.Add("@price", SqlDbType.Decimal).Value = request.Price;
            cmd.Parameters.Add("@stock", SqlDbType.Int).Value = request.Stock;

            conn.Open();
            int newId = Convert.ToInt32(cmd.ExecuteScalar());
            return StatusCode(201, new { message = "Barang berhasil dibuat", id = newId, data = request }); // 201 Created
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            return StatusCode(409, new { message = $"Kode barang '{request.ItemCode}' sudah ada (Duplikat)!" }); // 409 Conflict
        }
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        const string query = "DELETE FROM dbo.items WHERE id = @id";
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

        conn.Open();
        int affected = cmd.ExecuteNonQuery();
        if (affected == 0) return NotFound(new { message = $"Barang ID {id} tidak ditemukan." }); // 404

        return NoContent(); 
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] ItemRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            using var conn = new SqlConnection(_connectionString);
            const string query = @"
                UPDATE dbo.items 
                SET itemCode = @itemCode, itemName = @itemName, category = @category, price = @price, stock = @stock
                WHERE id = @id";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
            cmd.Parameters.Add("@itemCode", SqlDbType.VarChar, 50).Value = request.ItemCode;
            cmd.Parameters.Add("@itemName", SqlDbType.NVarChar, 150).Value = request.ItemName;
            cmd.Parameters.Add("@category", SqlDbType.NVarChar, 50).Value = request.Category;
            cmd.Parameters.Add("@price", SqlDbType.Decimal).Value = request.Price;
            cmd.Parameters.Add("@stock", SqlDbType.Int).Value = request.Stock;

            conn.Open();
            int affected = cmd.ExecuteNonQuery();
            if (affected == 0) return NotFound(new { message = $"Barang dengan ID {id} tidak ditemukan." }); // 404

            return NoContent(); 
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            return StatusCode(409, new { message = "Kode barang duplikat!" }); // 409 Conflict
        }
    }

}




