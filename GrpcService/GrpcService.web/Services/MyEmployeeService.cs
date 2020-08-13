using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcService.web.Data;
using GrpcService.Web.Protos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace GrpcService.web.Services
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MyEmployeeService : EmployeeService.EmployeeServiceBase
    {
        private readonly ILogger<MyEmployeeService> _logger;
        private readonly JwtTokenValidationService _jwtTokenValidationService;


        public MyEmployeeService(ILogger<MyEmployeeService> logger, JwtTokenValidationService jwtTokenValidationService)
        {
            _logger = logger;
            _jwtTokenValidationService = jwtTokenValidationService;
        }

        [AllowAnonymous]
        public override async Task<TokenResponse> CreateToken(TokenRequest request, ServerCallContext context)
        {
            var model = new UserModel
            {
                UserName = request.Username,
                Password = request.Password
            };

            var response = await _jwtTokenValidationService.GenerateTokenAsync(model);

            if (response.Success)
            {
                return new TokenResponse
                {
                    Token = response.Token,
                    Expiration = Timestamp.FromDateTime(response.Expriation),
                    Success = true
                };
            }

            return new TokenResponse
            {
                Success = false
            };
        }

        public override Task<EmployeeResponse> GetByNo(GetByNoRequest request, ServerCallContext context)
        {
            try
            {
                //if (true)
                //{
                //    var trailer = new Metadata
                //    {
                //        {"field", "No"},
                //        {"msg", "something went wrong ..."}
                //    };

                //    //throw new RpcException(status: Status.DefaultCancelled, "Server Error:");
                //    throw new RpcException(new Status(StatusCode.NotFound, "No is not found..."), trailer);
                //}

                var md = context.RequestHeaders;

                foreach (var item in md)
                {
                    _logger.LogInformation($"key:{item.Key} value:{item.Value}");
                }

                var employee = MemoryData.Employees.SingleOrDefault(r => r.No == request.No);

                if (employee != null)
                {
                    var response = new EmployeeResponse
                    {
                        Employee = employee
                    };

                    return Task.FromResult(response);
                }
            }
            catch (RpcException re)
            {
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw new RpcException(status:Status.DefaultCancelled, e.Message);
            }
            
            throw new Exception($"employee has not found with no : {request.No}");
        }


        public override async Task GetAll(GetAllRequest request, 
            IServerStreamWriter<EmployeeResponse> responseStream, 
            ServerCallContext context)
        {
            foreach (var employee in MemoryData.Employees)
            {
                await responseStream.WriteAsync(new EmployeeResponse
                {
                    Employee = employee
                });
            } 
        }

        public override async Task<AddPhotoResponse> AddPhoto(IAsyncStreamReader<AddPhotoRequest> requestStream, ServerCallContext context)
        {
            var md = context.RequestHeaders;

            foreach (var item in md)
            {
                _logger.LogInformation($"key:{item.Key} value:{item.Value}");
            }

            var data = new List<byte>();

            while (await requestStream.MoveNext())
            {
                Console.WriteLine($"received: {requestStream.Current.Data.Length} byte");
                data.AddRange(requestStream.Current.Data);
            }

            Console.WriteLine($"total received: {data.Count} byte");

            return new AddPhotoResponse
            {
                IsOk = true
            };
        }

        public override async Task SaveAll(IAsyncStreamReader<EmployeeRequest> requestStream, 
            IServerStreamWriter<EmployeeResponse> responseStream, 
            ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var employee = requestStream.Current.Employee;

                lock (this)
                {
                    MemoryData.Employees.Add(employee);
                }

                await responseStream.WriteAsync(new EmployeeResponse
                {
                    Employee = employee
                });
            }

            Console.WriteLine($"employees:");
            foreach (var employee in MemoryData.Employees)
            {
                Console.WriteLine(employee);
            }
        }

        public override Task<EmployeeResponse> Save(EmployeeRequest request, ServerCallContext context)
        {
            MemoryData.Employees.Add(request.Employee);
            
            var employee = new EmployeeResponse
            {
                Employee =  request.Employee
            };

            return Task.FromResult(employee);
        }

        
    }



}
