using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSwag.AspNetCore;
using Shop.Models;
using Maxx_Speed.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
        {
            app.UseOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI(config =>
            {
                config.DocumentTitle = "TodoAPI";
                config.SwaggerEndpoint("/swagger/v1/swagger.json", "TodoAPI v1");
                config.RoutePrefix = "swagger";
                config.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            });
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        // Categories
        app.MapGet("/categories", async (ApplicationDbContext db) =>
        {
            var categories = await db.Categories
                .Include(c => c.Cars)
                .ToListAsync();
            return Results.Ok(categories);
        });

        app.MapPost("/categories", async (ApplicationDbContext db, Category category) =>
        {
            db.Categories.Add(category);
            await db.SaveChangesAsync();
            return Results.Created($"/categories/{category.Id}", category);
        });

        app.MapPut("/categories/{id}", async (int id, ApplicationDbContext db, Category inputCategory) =>
        {
            var category = await db.Categories.FindAsync(id);

            if (category is null) return Results.NotFound();

            category.Name = inputCategory.Name;
            category.Description = inputCategory.Description;

            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        app.MapDelete("/categories/{id}", async (int id, ApplicationDbContext db) =>
        {
            if (await db.Categories.FindAsync(id) is Category category)
            {
                db.Categories.Remove(category);
                await db.SaveChangesAsync();
                return Results.NoContent();
            }

            return Results.NotFound();
        });

        // Cars
        app.MapGet("/Cars", async (ApplicationDbContext db) =>
        {
            var cars = await db.Cars
                .Include(c => c.Category)  // Подключение категории
                .ToListAsync();
            return Results.Ok(cars);
        });

        app.MapPost("/Cars", async (ApplicationDbContext db, Car car) =>
        {
            db.Cars.Add(car);
            await db.SaveChangesAsync();
            return Results.Created($"/Cars/{car.Id}", car);
        });

        app.MapPut("/Cars/{id}", async (int id, ApplicationDbContext db, Car inputCar) =>
        {
            var car = await db.Cars.FindAsync(id);

            if (car is null) return Results.NotFound();

            car.Name = inputCar.Name;
            car.Price = inputCar.Price;
            car.CategoryId = inputCar.CategoryId;  // Обновление связи с категорией
            car.Description = inputCar.Description;
            car.ImagePath = inputCar.ImagePath;
            car.Amount = inputCar.Amount;

            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        app.MapDelete("/Cars/{id}", async (int id, ApplicationDbContext db) =>
        {
            if (await db.Cars.FindAsync(id) is Car car)
            {
                db.Cars.Remove(car);
                await db.SaveChangesAsync();
                return Results.NoContent();
            }

            return Results.NotFound();
        });
// Orders
app.MapGet("/orders", async (ApplicationDbContext db) =>
{
    var orders = await db.Orders
        .Include(o => o.OrderCars) 
        .ThenInclude(oc => oc.Car)
        .ToListAsync();
    return Results.Ok(orders);
});

app.MapPost("/orders", async (ApplicationDbContext db, Order order) =>
{
    foreach (var orderCar in order.OrderCars)  
    {
        
        orderCar.Car = await db.Cars.FindAsync(orderCar.Car.Id);  
    }

    db.Orders.Add(order);
    await db.SaveChangesAsync();
    return Results.Created($"/orders/{order.Id}", order);
});

app.MapPut("/orders/{id}", async (int id, ApplicationDbContext db, Order inputOrder) =>
{
    var order = await db.Orders
        .Include(o => o.OrderCars)  
        .FirstOrDefaultAsync(o => o.Id == id);

    if (order is null) return Results.NotFound();

    order.Status = inputOrder.Status;

    // Очищаем старые записи OrderCar перед обновлением
    order.OrderCars.Clear();  // Используем правильное название свойства
    foreach (var orderCar in inputOrder.OrderCars)
    {
        var car = await db.Cars.FindAsync(orderCar.Car.Id);
        if (car is not null)
        {
            order.OrderCars.Add(new OrderCar
            {
                Car = car,
                Amount = orderCar.Amount
            });
        }
    }

    await db.SaveChangesAsync();
    return Results.NoContent();
});


app.MapDelete("/orders/{id}", async (int id, ApplicationDbContext db) =>
        {
            var order = await db.Orders.FindAsync(id);

            if (order is null) return Results.NotFound();

            db.Orders.Remove(order);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });


        // OrderCars
        app.MapGet("/ordercars", async (ApplicationDbContext db) =>
        {
            var orderCars = await db.OrderCars
                .Include(oc => oc.Car)
                .Include(oc => oc.Order)
                .ToListAsync();
            return Results.Ok(orderCars);
        });

        app.MapPost("/ordercars", async (ApplicationDbContext db, OrderCar inputOrderCar) =>
        {
            var orderCar = await db.OrderCars.FirstOrDefaultAsync(o => o.Id == inputOrderCar.Id);

            if (orderCar is null) return Results.NotFound();

            orderCar.CarId = inputOrderCar.CarId;
            orderCar.OrderId = inputOrderCar.OrderId;
            orderCar.Amount = inputOrderCar.Amount;

            db.OrderCars.Add(orderCar);
            await db.SaveChangesAsync();
            return Results.Created($"/ordercars/{orderCar.Id}", orderCar);
        });

        app.MapPut("/ordercars/{id}", async (int id, ApplicationDbContext db, OrderCar inputOrderCar) =>
        {
            var orderCar = await db.OrderCars.FindAsync(id);

            if (orderCar is null) return Results.NotFound();

            var car = await db.Cars.FindAsync(inputOrderCar.CarId);
            var order = await db.Orders.FindAsync(inputOrderCar.OrderId);

            if (car is null || order is null) return Results.BadRequest("Invalid car or order ID.");

            orderCar.Amount = inputOrderCar.Amount;
            orderCar.CarId = inputOrderCar.CarId;
            orderCar.OrderId = inputOrderCar.OrderId;

            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        app.MapDelete("/ordercars/{id}", async (int id, ApplicationDbContext db) =>
        {
            if (await db.OrderCars.FindAsync(id) is OrderCar orderCar)
            {
                db.OrderCars.Remove(orderCar);
                await db.SaveChangesAsync();
                return Results.NoContent();
            }

            return Results.NotFound();
        });






        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();
        app.MapRazorPages();
        app.Run();