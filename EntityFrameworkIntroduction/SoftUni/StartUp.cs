using Microsoft.EntityFrameworkCore;
using SoftUni.Data;
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
            Console.WriteLine(GetEmployeesFullInformation(context));

            //04.Employees with Salary Over 50 000
            Console.WriteLine(GetEmployeesWithSalaryOver50000(context));

            //05.Employees from Research and Development
            Console.WriteLine(GetEmployeesFromResearchAndDevelopment(context));
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
    }
}