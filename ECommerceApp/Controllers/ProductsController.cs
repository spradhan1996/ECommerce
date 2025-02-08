using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ECommerceApp.Models;
using ECommerceApp.Service;
using ECommerceApp.Interface;

namespace ECommerceApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context, IUserService UserService)
        {
            _context = context;
            _userService = UserService;
        }

        // GET: Products
        //public async Task<IActionResult> Index()
        //{
        //    try
        //    {
        //        return View(await _context.Products.ToListAsync());
        //    }
        //    catch (Exception ex)
        //    {
        //        return View(ex.Message);
        //    }
        //}

        //public async Task<IActionResult> Index()
        //{
        //    try
        //    {
        //        var products = await _context.Products.ToListAsync();
        //        var currentUser = await _userService.GetCurrentUserAsync();  // Assuming you have a method to fetch the current user

        //        // Create a view model that holds both Products and the current User
        //        var viewModel = new IndexViewModel
        //        {
        //            Products = products,
        //            CurrentUser = currentUser
        //        };

        //        return View(viewModel);
        //    }
        //    catch (Exception ex)
        //    {
        //        return View(ex.Message);  // Handle error and display the exception message
        //    }
        //}

        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _context.Products.ToListAsync();
                var currentUser = await _userService.GetCurrentUserAsync();

                // Fetch product images and group by ProductId
                var productImages = await _context.ProductImages
                    .GroupBy(pi => pi.ProductId)
                    .ToDictionaryAsync(g => g.Key, g => g.ToList());

                var viewModel = new IndexViewModel
                {
                    Products = products,
                    CurrentUser = currentUser,
                    ProductImages = productImages
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return View("Error", ex.Message);
            }
        }


        // GET: Products/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var product = await _context.Products
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(product);
        //}

        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                // Fetch product details
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                // Fetch associated product images
                var productImages = await _context.ProductImages
                    .Where(pi => pi.ProductId == id)
                    .ToListAsync();

                // Create ViewModel to pass product and images to the view
                var viewModel = new ProductDetailsViewModel
                {
                    Product = product,
                    ProductImages = productImages
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return View("Error", ex.Message);
            }
        }


        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,Name,Description,Price,StockQuantity,Category")] Product product)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(product);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(product);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Price,StockQuantity,Category")] Product product, List<IFormFile> ProductImages)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();

                if (ProductImages != null && ProductImages.Count > 0)
                {
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    foreach (var image in ProductImages)
                    {
                        if (image.Length > 0)
                        {
                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                            var filePath = Path.Combine(uploadPath, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await image.CopyToAsync(stream);
                            }

                            // Save image path in the database
                            var productImage = new ProductImage
                            {
                                ProductId = product.Id,
                                ImageUrl = "/images/" + fileName
                            };

                            _context.ProductImages.Add(productImage);
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        // GET: Products/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var product = await _context.Products.FindAsync(id);
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(product);
        //}

        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                // Fetch product and attachment details from the database
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                // Fetch product images (if applicable)
                var productImages = await _context.ProductImages
                    .Where(pi => pi.ProductId == id)
                    .ToListAsync();

                // Creating ViewModel to pass product and images to the view
                var viewModel = new EditProductViewModel
                {
                    Product = product,
                    ProductImages = productImages
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return View("Error", ex.Message);
            }
        }



        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,StockQuantity,Category")] Product product)
        //{
        //    if (id != product.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(product);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!ProductExists(product.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(product);
        //}


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,StockQuantity,Category")] Product product, List<IFormFile> ProductImages, List<int> RemovedImageIds)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

           
                try
                {
                    // Update the product details
                    _context.Update(product);

                    // Remove images that have been marked for deletion
                    if (RemovedImageIds != null && RemovedImageIds.Count > 0)
                    {
                        var imagesToRemove = _context.ProductImages.Where(p => RemovedImageIds.Contains(p.Id)).ToList();
                        _context.ProductImages.RemoveRange(imagesToRemove);
                    }

                    // Add new product images
                    if (ProductImages != null && ProductImages.Count > 0)
                    {
                        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }

                        foreach (var image in ProductImages)
                        {
                            if (image.Length > 0)
                            {
                                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                                var filePath = Path.Combine(uploadPath, fileName);

                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await image.CopyToAsync(stream);
                                }

                                // Save image path in the database
                                var productImage = new ProductImage
                                {
                                    ProductId = product.Id,
                                    ImageUrl = "/images/" + fileName
                                };

                                _context.ProductImages.Add(productImage);
                            }
                        }
                    }

                    // Save all changes to the database
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

           


        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

             return View(product);
            //return RedirectToAction(nameof(Index));
        }

        // POST: Products/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var product = await _context.Products.FindAsync(id);
        //    if (product != null)
        //    {
        //        _context.Products.Remove(product);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                // Fetch associated images
                var productImages = await _context.ProductImages.Where(pi => pi.ProductId == id).ToListAsync();

                // Delete image files from the server
                foreach (var image in productImages)
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }

                    _context.ProductImages.Remove(image);
                }

                // Save changes after deleting images
                await _context.SaveChangesAsync();

                // Now delete the product
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index)); // Redirect after successful deletion
            }
            catch (Exception ex)
            {
                return View("Error", new { message = ex.Message });
            }
        }


        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
