using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualBasic;
using SoftUni.Data;
using SoftUni.Models;
using System.Linq;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SoftUni
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            //03.Employees Full Information
            SoftUniContext context = new SoftUniContext();
            //Console.WriteLine(GetEmployeesFullInformation(context));

            //04.Employees with Salary Over 50 000
            //Console.WriteLine(GetEmployeesWithSalaryOver50000(context));

            //05.Employees from Research and Development
            //Console.WriteLine(GetEmployeesFromResearchAndDevelopment(context));

            //06.Adding a New Address and Updating Employee
            //Console.WriteLine(AddNewAddressToEmployee(context));

            //07.Employees and Projects
            //Console.WriteLine(GetEmployeesInPeriod(context));

            //08.Addresses by Town
            //Console.WriteLine(GetAddressesByTown(context));

            //09.Employee 147
            //Console.WriteLine(GetEmployee147(context));

            //10.Departments with More Than 5 Employees
            //Console.WriteLine(GetDepartmentsWithMoreThan5Employees(context));

            //11.Find Latest 10 Projects
            //Console.WriteLine(GetLatestProjects(context));

            //12.Increase Salaries
            Console.WriteLine(IncreaseSalaries(context));

        }

        //03
        public static string GetEmployeesFullInformation(SoftUniContext context)
        {
            // Option 1 for 03.Employees Full Information

            //return string.Join(Environment.NewLine, context.Employees
            //    .Select(e => $"{e.FirstName} {e.LastName} {e.MiddleName} {e.JobTitle} {e.Salary:F2}").ToList());

            // Option 2 for 03.Employees Full Information

            var employees = context.Employees
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.MiddleName,
                    e.JobTitle,
                    e.Salary
                })
                .ToList();

            StringBuilder sb = new StringBuilder();
            foreach (var e in employees)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} {e.MiddleName} {e.JobTitle} {e.Salary:F2}");
            }

            return sb.ToString().TrimEnd();
        }

        //04
        public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
        {
            var richEmployees = context.Employees
                .Where(e => e.Salary > 50000)
                .Select(e => new
                {
                    e.FirstName,
                    e.Salary
                })
                .OrderBy(e => e.FirstName)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var r in richEmployees)
            {
                sb.AppendLine($"{r.FirstName} - {r.Salary:F2}");
            }

            return sb.ToString().TrimEnd();
        }

        //05
        public static string GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
        {
            var employees = context.Employees
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.Salary,
                    e.Department
                })
                .Where(e => e.Department.Name == "Research and Development")
                .OrderBy(e => e.Salary)
                .ThenByDescending(e => e.FirstName)
                .ToList() ;
            
            StringBuilder sb = new StringBuilder();

            foreach (var e in employees)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} from {e.Department.Name} - ${e.Salary:F2}");
            }

            return sb.ToString().TrimEnd();
        }

        //06
        public static string AddNewAddressToEmployee(SoftUniContext context)
        {
            Address newAddress = new Address()
            {
                AddressText = "Vitoshka 15",
                TownId = 4
            };

            var nakov = context.Employees
                .FirstOrDefault(e => e.LastName == "Nakov");

            if (nakov != null)
            {
                nakov.Address = newAddress;
                context.SaveChanges();
            }

            var employees = context.Employees
                .OrderByDescending(e => e.AddressId)
                .Take(10)
                .Select(e => e.Address.AddressText)
                .ToList();

            return string.Join(Environment.NewLine, employees);
        }

        //07
        public static string GetEmployeesInPeriod(SoftUniContext context)
        {
            var result = context.Employees
                .Take(10)
                .Select(e => new
                {
                    EmployeeName = $"{e.FirstName} {e.LastName}",
                    ManagerName = $"{e.Manager.FirstName} {e.Manager.LastName}",
                    Projects = e.EmployeesProjects
                        .Where(ep => ep.Project.StartDate.Year >= 2001 &&
                                     ep.Project.StartDate.Year <= 2003)
                                     .Select(ep => new {
                                        ProjectName = ep.Project.Name,
                                        ep.Project.StartDate,
                                            EndDate = ep.Project.EndDate.HasValue ? 
                                            ep.Project.EndDate.Value.ToString("M/d/yyyy h:mm:ss tt") :
                                            "not finished"
                                     })
                });

            StringBuilder sb = new StringBuilder();

            foreach (var e in result)
            {
                sb.AppendLine($"{e.EmployeeName} - Manager: {e.ManagerName}");
                if (e.Projects.Any())
                {
                    foreach (var p in e.Projects)
                    {
                        sb.AppendLine($"--{p.ProjectName} - {p.StartDate:M/d/yyyy h:mm:ss tt} - "
                            + $"{p.EndDate}");
                    }

                }
            }

            return sb.ToString().TrimEnd();
        }

        //08
        public static string GetAddressesByTown(SoftUniContext context)
        {
            var addresses = context.Addresses
                .Take(10)
                .Select(a => new
                {
                    a.AddressText,
                    TownName = a.Town.Name,
                    EmployeeCount = a.Employees.Count()
                })
                .OrderByDescending(t => t.EmployeeCount)
                .ThenBy(tn => tn.TownName)
                .ThenBy(at => at.AddressText);

            StringBuilder sb = new StringBuilder();

            foreach (var a in addresses)
            {
                sb.AppendLine($"{a.AddressText}, {a.TownName} - {a.EmployeeCount} employees");
            }

            return sb.ToString().TrimEnd();
        }

        //09
        public static string GetEmployee147(SoftUniContext context)
        {
            var e = context.Employees
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.JobTitle,
                    e.EmployeesProjects,
                    Project = e.EmployeesProjects.Select(p => p.Project),
                    e.EmployeeId
                })
                .FirstOrDefault(e => e.EmployeeId == 147);

            StringBuilder sb = new StringBuilder();

            
                sb.AppendLine($"{e.FirstName} {e.LastName} - {e.JobTitle}");

                foreach (var ep in e.Project.OrderBy(p => p.Name))
                {
                    sb.AppendLine(ep.Name);
                }

            return sb.ToString().TrimEnd();
        }

        //10
        public static string GetDepartmentsWithMoreThan5Employees(SoftUniContext context)
        {
            var dept = context.Departments.Where(d => d.Employees.Count() > 5)
                .Select(d => new
                {
                    EmployeeCount = d.Employees.Count(),
                    d.Name,
                    ManagerName = d.Manager.FirstName + " " + d.Manager.LastName
                })
                .OrderBy(c => c.EmployeeCount)
                .ThenBy(c => c.Name)
                .ToList();

            var employees = context.Employees
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.JobTitle
                })
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var d in dept)
            {
                sb.AppendLine($"{d.Name} - {d.ManagerName}");

                foreach (var e in employees)
                {
                    sb.AppendLine($"{e.FirstName} {e.LastName} - {e.JobTitle}");
                }
            }
                
            return sb.ToString().TrimEnd();
        }

        //11
        public static string GetLatestProjects(SoftUniContext context)
        {
            var projects = context.Projects
                .OrderByDescending(p => p.StartDate)
                .Take(10)
                .Select(p => new
                {
                    p.Name,
                    p.Description,
                    p.StartDate
                })
                .OrderBy(p => p.Name)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var p in projects)
            {
                sb.AppendLine($"{p.Name}");
                sb.AppendLine($"{p.Description}");
                sb.AppendLine($"{p.StartDate.ToString("M/d/yyyy h:mm:ss tt")}");
            }

            return sb.ToString().TrimEnd();
        }

        //12
        public static string IncreaseSalaries(SoftUniContext context)
        {
            string[] searchedDept = {"Engineering", "Tool Design", "Marketing", "Information Services"};

            var departments = context.Departments
                .Where(d => d.Name == "Engineering" || d.Name == "Tool Design" || 
                d.Name == "Marketing" || d.Name == "Information Services")
                .ToList();

            foreach (var employee in context.Employees.Where(em => searchedDept.Contains(em.Department.Name)))
            {
                employee.Salary *= 1.12m;
            }

                context.SaveChanges();
             
            var employees = context.Employees
             .Where(e => searchedDept.Contains(e.Department.Name))
             .OrderBy(e => e.FirstName)
             .ThenBy(e => e.LastName)
             .Select(e => new
             {
                 e.FirstName,
                 e.LastName,
                 e.Salary
             })
             .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var e in employees)
            {
                 sb.AppendLine($"{e.FirstName} {e.LastName} (${e.Salary:F2})");
                
            }

            return sb.ToString().TrimEnd();
        }
    }
}