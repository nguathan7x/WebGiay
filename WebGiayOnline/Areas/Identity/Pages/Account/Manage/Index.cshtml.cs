// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebGiayOnline.Models;
using WebGiayOnline.Services;
using Microsoft.AspNetCore.Hosting; // ⚠️ Phải có dòng này
using Microsoft.Extensions.Hosting; 

namespace WebGiayOnline.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly CloudinaryService _cloudinaryService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            CloudinaryService cloudinaryService,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _cloudinaryService = cloudinaryService;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }


        public string AvatarUrl { get; set; }

        //public string? CoverImageUrl { get; set; }
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Full Name")]
            public string FullName { get; set; }

            [Display(Name = "Age")]
            public string Age { get; set; }

            [Display(Name = "Address")]
            public string Address { get; set; }

            [Display(Name = "Ảnh đại diện")]
            public IFormFile? AvatarImage { get; set; }

            public string AvatarUrl { get; set; }

            //[Display(Name = "Ảnh bìa")]
            //public IFormFile? CoverImage { get; set; }

            //public string? CoverImageUrl { get; set; }



        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;
            AvatarUrl = user.AvatarUrl;
            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                 FullName = user.FullName,
                Age = user.Age,
                Address = user.Address,
                AvatarUrl = user.AvatarUrl,
            };
            
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            if (user.FullName != Input.FullName)
            {
                user.FullName = Input.FullName;
            }
            if (user.Age != Input.Age)
            {
                user.Age = Input.Age;
            }
            if (user.Address != Input.Address)
            {
                user.Address = Input.Address;
            }

            if (Input.AvatarImage != null)
            {
                // Nếu có ảnh cũ, thì xóa trước
                if (!string.IsNullOrEmpty(user.AvatarPublicId))
                {
                    await _cloudinaryService.DeleteImageAsync(user.AvatarPublicId);
                }

                // Tải ảnh mới
                var uploadResult = await _cloudinaryService.UploadImageAsync(Input.AvatarImage);
                user.AvatarUrl = uploadResult.imageUrl;
                user.AvatarPublicId = uploadResult.publicId;
            }

            //if (Input.CoverImage != null && Input.CoverImage.Length > 0)
            //{
            //    // Xử lý lưu ảnh lên wwwroot hoặc cloud
            //    var fileName = Guid.NewGuid() + Path.GetExtension(Input.CoverImage.FileName);
            //    var path = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", fileName);
            //    using (var stream = new FileStream(path, FileMode.Create))
            //    {
            //        await Input.CoverImage.CopyToAsync(stream);
            //    }

            //    user.CoverImageUrl = "/uploads/" + fileName;
            //}


            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                StatusMessage = "Unexpected error when trying to update profile.";
                return RedirectToPage();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
