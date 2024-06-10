﻿using FormContato.DTOs;
using FormContato.Repositories;
using Microsoft.AspNetCore.Mvc;
using FormContato.Services;
using FormContato.Models;
using AutoMapper;

namespace FormContato.Controllers;
public class RegisterController : Controller
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PasswordHasher _hasher;

    public RegisterController(IMapper mapper, IUnitOfWork unitOfWork, PasswordHasher hasher)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _hasher = hasher;
    }

    public ActionResult Index(LoginDTO login = null)
    {
        var model = login != null ? new RegisterDTO { Email = login.Email, Password = login.Password } : new RegisterDTO();
        return View("Register");
    }

    [HttpPost]
    public async Task<ActionResult> Create(RegisterDTO user)
    {
        try
        {

            var checkUser = _unitOfWork.UserRepository.Get(u => u.Email == user.Email);

            if (checkUser !=  null)
            {
                return BadRequest("This user is already registered.");
            }
 
            _hasher.HashPassword(user.Password);

            var newUser = _mapper.Map<UserModel>(user);

            newUser.Password = _hasher.Password;
            newUser.Salt = _hasher.Salt;
            newUser.Role = RoleEnum.User;

            _unitOfWork.UserRepository.Create(newUser);
            await _unitOfWork.CommitAsync();

            var handler = new JwtHandler();

            var token = handler.GenerateToken(newUser);

            Response.Headers.Append("Authorization", "Bearer " + token);
            return RedirectToAction("Index", "Dashboard"); // redirecionar para página inicial de usuário logado
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Error", "Home");
        }
    }

}
