using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using Thinktecture.Relay.Server.Dto;
using Thinktecture.Relay.Server.Http.Filters;
using Thinktecture.Relay.Server.Repository;
using Thinktecture.Relay.Server.Security;

namespace Thinktecture.Relay.Server.Controller.ManagementWeb
{
	[Authorize(Roles = "Admin")]
	[ManagementWebModuleBindingFilter]
	[NoCache]
	public class UserController : ApiController
	{
		private readonly IUserRepository _userRepository;
		private readonly IPasswordComplexityValidator _passwordComplexityValidator;

		public UserController(IUserRepository userRepository, IPasswordComplexityValidator passwordComplexityValidator)
		{
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_passwordComplexityValidator = passwordComplexityValidator ?? throw new ArgumentNullException(nameof(passwordComplexityValidator));
		}

		[AllowAnonymous]
		[HttpPost]
		[ActionName("firsttime")]
		public IHttpActionResult CreateFirstUser(CreateUser user)
		{
			if (_userRepository.Any())
			{
				return new StatusCodeResult(HttpStatusCode.Forbidden, Request);
			}

			if (!CheckPassword(user, out var result))
			{
				return result;
			}

			return Create(user);
		}

		[HttpGet]
		[ActionName("users")]
		public IEnumerable<User> List()
		{
			return _userRepository.List();
		}

		[HttpPost]
		[ActionName("user")]
		public IHttpActionResult Create(CreateUser user)
		{
			if (user == null)
			{
				return BadRequest();
			}

			if (String.IsNullOrWhiteSpace(user.UserName))
			{
				return Ok(new { Errors = new [] { "User name was not provided.", } });
			}

			if (String.IsNullOrWhiteSpace(user.Password))
			{
				return Ok(new { Errors = new [] { "Password was not provided.", } });
			}

			if (!CheckPassword(user, out var result))
			{
				return result;
			}

			_userRepository.Create(user.UserName, user.Password);

			return Ok();
		}

		[HttpGet]
		[ActionName("user")]
		public IHttpActionResult Get(Guid id)
		{
			var user = _userRepository.Get(id);

			if (user == null)
			{
				return NotFound();
			}

			return Ok(user);
		}

		[HttpDelete]
		[ActionName("user")]
		public IHttpActionResult Delete(Guid id)
		{
			if (_userRepository.Delete(id))
				return Ok();

			return Ok(new {Errors = new [] { "User was not found." } });
		}

		[HttpPut]
		[ActionName("user")]
		public IHttpActionResult Update(UpdateUser user)
		{
			if (user == null)
			{
				return BadRequest();
			}

			// OldPassword needs to be correct
			var authenticatedUser = _userRepository.Authenticate(user.UserName, user.CurrentPassword);
			if (authenticatedUser == null)
			{
				return Ok(new { Errors = new [] { "Current password is not correct.", } });
			}

			if (user.CurrentPassword == user.Password)
			{
				return Ok(new { Errors = new [] { "New password must be different from current one.", } });
			}

			if (!CheckPassword(user, out var error))
			{
				return error;
			}

			var result = _userRepository.Update(authenticatedUser.Id, user.Password);

			return result ? (IHttpActionResult) Ok() : BadRequest();
		}

		[HttpGet]
		[ActionName("userNameAvailability")]
		public IHttpActionResult GetUserNameAvailability(string userName)
		{
			if (_userRepository.IsUserNameAvailable(userName))
			{
				return Ok();
			}

			return Conflict();
		}

		private bool CheckPassword(CreateUser user, out IHttpActionResult httpActionResult)
		{
			httpActionResult = null;

			// validate password complexity by other rules
			if (!_passwordComplexityValidator.ValidatePassword(user.UserName, user.Password, out var errorMessages))
			{
				httpActionResult = Ok(new { Errors = errorMessages });
				return false;
			}

			return true;
		}
	}
}
