using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using GrpcService.web.Protos;
using GrpcService.Web.Protos;

namespace GrpcService.web.Data
{
    public class MemoryData
    {

        public static List<Employee> Employees = new List<Employee>
        {
            new Employee
            {
                Id = 1,
                No = 111,
                FirstName = "zhangsan",
                LastName = "a",
                //Salary = 2000
                MonthSalary = new MonthSalary
                {
                    Basic = 4584f,
                    Bonus = 334.2f
                },
                Status = EmployeeStatus.Normal,
                LastModied = Timestamp.FromDateTime(DateTime.UtcNow)
            },
            new Employee
            {
                Id = 2,
                No = 222,
                FirstName = "xiaoming",
                LastName = "b",
                //Salary = 3000
                MonthSalary = new MonthSalary
                {
                    Basic = 3565f,
                    Bonus = 1233.9f
                },
                Status = EmployeeStatus.Resigned,
                LastModied = Timestamp.FromDateTime(DateTime.UtcNow)
            },
            new Employee
            {
                Id = 3,
                No = 333,
                FirstName = "lisi",
                LastName = "c",
                //Salary = 4000
                MonthSalary = new MonthSalary
                {
                    Basic = 3223f,
                    Bonus = 456.7f
                },
                Status = EmployeeStatus.Retired,
                LastModied = Timestamp.FromDateTime(DateTime.UtcNow)
            }
        };

    }
}
