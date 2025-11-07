Name: Rebecca Bomgardner [DevOpsCapstoneProject]
(https://github.com/rbom482/DevOpsCapstoneProject) is the GitHub repo.


LogiTrack is a backend Order Management System for companies with lots of fulfillment centers to track inventories and orders.  It runs on a secure REST API and supports:

Inventory Management
Order handling: 
User Authentication
Performance Monitoring
State Persistence

3 Functionalities


Order several items  
Check stock levels, track order status/change or cancel orders  
Take things out of stock automatically

Taking Care of Your Inventory

Works with warehouses and distribution centers
Alerts for low inventory and real-time stock changes 
Caching to keep system up-to-date 
Search and filtering by location

 Role-based access control
Password rules and account lockouts 
In-memory caching makes queries faster   
Gzip compression and database optimization to make responses better. 
 Built-in health checks and diagnostics.

 

 Step 2: The Development Process

1. Many-to-Many Relationships (Orders ↔ InventoryItems)
 The "OrderItem" join entity stopped circular references from happening. Used `HasMany(). WithMany() and loading eagerly with .Include() to make sure DB calls are fast.

 2. How well it works when it's busy
 The earliest response times were slow, taking between 300 and 500 milliseconds.  Changed reads to "AsNoTracking()", implemented caching with smart invalidation, and switched on connection pooling.  Brought cached answers down to less than 50 milliseconds.

 3. Making sure the API is safe without breaking anything
 Added JWT authentication to the routes that were already there.  Made a separate `AuthController`, added `[Authorize]` attributes where they were needed, and built in middleware to make it easier to check tokens and handle failures.


 Planned First
Built in Phases
Tested as I Went
Monitored Performance

Copilot Helped Me:

Look for and fix circular navigation properties in EF
Improve async patterns in the caching service 
Refactor controller logic to make it easier to read and maintain 
Offer ideas for making the system faster and safer (like password rules and setting up JWT)


 Step 3: Growth and Performance

Made better queries

 * `AsNoTracking()` made reading faster.
 * Using Include() gets rid of N+1 query issues.
 * Used 128 DB connections to handle a lot of queries at the same time

 Plan for Caching

Inventory: Cached for an hour and invalidated every 30 minutes 
Orders: Cached for 2 to 5 minutes, and the cache automatically expires when updates are made 
Pattern-based invalidation to keep things up-to-date 
80–95% cache hit rate on repeated requests

Made Responses Better

 Gzip compression makes the payload smaller. 
Selective data loading ensures that clear and concise answers
It is easier to work with huge result sets when pagination is supported.


 Step 4: Plan the API and the business logic

Data Models:

 ```csharp public class InventoryItem { public int ItemId { get; set; } }
   [Required] public string Name { get; set; }
   public int Amount { get; set; }
   [Required] public string Location { get; set; }
   public ICollection<OrderItem> OrderItems { get; set; }

 public class Order { public int OrderId { get; set; }
   [Required]  public string CustomerName { get; set; }
   public DateTime DatePlaced { get; set; }
   public ICollection<OrderItem> OrderItems { get; set; }
   public string UserId { get; set; }
   public ApplicationUser User { get; set; }

 OrderItemId, OrderId, InventoryItemId, QuantityOrdered, Order, and InventoryItem are some of the properties of the OrderItem class.

 Important API Route Highlights

AuthController
 `POST /register` – Create a new user 
`POST /login` – Check the user's identity and get a token

InventoryController
 'GET` – List all items (with caching)
`GET /{id}` – Get item by ID
 `POST` – Add new item 
`PUT /{id}` – Update item 
`DELETE /{id}` – Delete item

OrderController
`GET` – Show all user orders (with caching) 
 "POST" means to make a new order
 "PUT /{id}" means to change an order that is already there
 "DELETE /{id}" means to cancel an order.


Checking and Protecting

Creating a JWT Bearer token in `Program.cs` 
A strong password policy and freezing accounts after failed logins 
Tokens are only good for 24 hours 
All sensitive endpoints need `[Authorize]` 
Annotations are used to examine data input 
Protection from SQL injection

 Step 5: Final Checks, Testing, and Watching

Testing 
 Register, log in, place orders, and check stock 
Role-based security and protected route access 
Performance tests and cache verification

Performance Highlights

 Cached responses: ~80–95% faster (≤ 50ms) 
Query optimization: ~30–50% better
Gzip compression makes payloads 40% to 60% smaller.
 Can manage a lot of connections at once thanks to connection pooling

 Health and Checking

 Both `/health` and `/api/system/health` are working nicely. You can see real-time stats at `/api/performance/metrics`.
 Keeps an eye on the database's status, response time, and cache hits



 Final Thoughts


Built a production-ready API with real world features
Learned how to use Entity Framework relationships, migrations, and optimization 
Set up secure authentication with JWT and check the input 
Saw real performance gains via caching, compression, and optimizing queries. Wrote clear documentation and kept the code clean and easy to test.

What I Learned

 I learned the most about how to make a sluggish, clumsy API operate better by using clever caching and queries that are faster.  It felt like a huge win to lower response times from 500ms to less than 50ms, especially because I knew it was based on genuine data.