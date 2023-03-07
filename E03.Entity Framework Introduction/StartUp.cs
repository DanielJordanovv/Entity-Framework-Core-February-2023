namespace SoftUni;

using System.Globalization;
using System.Text;

using Microsoft.EntityFrameworkCore;

using Data;
using Models;

public class StartUp
{
    static void Main(string[] args)
    {
        SoftUniContext dbContext = new SoftUniContext();
        string result = RemoveTown(dbContext);
        Console.WriteLine(result);
    }
    public static string GetEmployeesFullInformation(SoftUniContext context)
    {
        StringBuilder sb = new StringBuilder();
        Employee[] employees = context.Employees.OrderBy(x => x.EmployeeId).ToArray();
        foreach (Employee e in employees)
        {
            sb.AppendLine($"{e.FirstName} {e.LastName} {e.MiddleName} {e.JobTitle} {e.Salary:f2}");
        }
        return sb.ToString().TrimEnd();

    }
    public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
    {
        StringBuilder sb = new StringBuilder();
        Employee[] employees = context.Employees.Where(x => x.Salary > 50000).OrderBy(x => x.FirstName).ToArray();
        foreach (var e in employees)
        {
            sb.AppendLine($"{e.FirstName} - {e.Salary:f2}");
        }
        return sb.ToString().TrimEnd();
    }
    public static string GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
    {
        StringBuilder sb = new StringBuilder();
        var employees = context.Employees.Where(e => e.Department.Name == "Research and Development").Select(e => new { e.FirstName, e.LastName, DepartmentName = e.Department.Name, e.Salary }).OrderBy(x => x.Salary).ThenByDescending(x => x.FirstName).ToArray();
        foreach (var e in employees)
        {
            sb.AppendLine($"{e.FirstName} {e.LastName} from {e.DepartmentName} - ${e.Salary:f2}");
        }
        return sb.ToString().Trim();
    }
    public static string AddNewAddressToEmployee(SoftUniContext context)
    {
        Address address = new Address()
        {
            AddressText = "Vitoshka 15",
            TownId = 4
        };
        Employee? employee = context.Employees
            .FirstOrDefault(e => e.LastName == "Nakov");
        employee.Address = address;

        context.SaveChanges();
        var employeeAddresses = context.Employees
            .OrderByDescending(e => e.AddressId)
            .Take(10)
            .Select(e => e.Address!.AddressText)
            .ToArray();
        return String.Join(Environment.NewLine, employeeAddresses);
    }
    public static string GetEmployeesInPeriod(SoftUniContext context)
    {
        StringBuilder sb = new StringBuilder();
        var employeesWithProjects = context.Employees
            .Take(10)
            .Select(e => new
            {
                e.FirstName,
                e.LastName,
                ManagerFirstName = e.Manager!.FirstName,
                ManagerLastName = e.Manager!.LastName,
                Projects = e.EmployeesProjects
                    .Where(ep => ep.Project.StartDate.Year >= 2001 &&
                                 ep.Project.StartDate.Year <= 2003)
                    .Select(ep => new
                    {
                        ProjectName = ep.Project.Name,
                        StartDate = ep.Project.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture),
                        EndDate = ep.Project.EndDate.HasValue
                            ? ep.Project.EndDate.Value.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)
                            : "not finished"
                    })
                    .ToArray()
            })
            .ToArray();

        foreach (var e in employeesWithProjects)
        {
            sb
                .AppendLine($"{e.FirstName} {e.LastName} - Manager: {e.ManagerFirstName} {e.ManagerLastName}");

            foreach (var p in e.Projects)
            {
                sb
                    .AppendLine($"--{p.ProjectName} - {p.StartDate} - {p.EndDate}");
            }
        }
        return sb.ToString().Trim();
    }
    public static string GetAddressesByTown(SoftUniContext context)
    {
        StringBuilder sb = new StringBuilder();
        var addresses = context.Addresses
               .Select(x => new { Address = x.AddressText, Town = x.Town.Name, Employees = x.Employees.Count })
               .OrderByDescending(x => x.Employees).ThenBy(x => x.Town).ThenBy(x => x.Address)
               .Take(10).ToArray();
        foreach (var a in addresses)
        {
            sb.AppendLine($"{a.Address}, {a.Town} - {a.Employees} employees");
        }
        return sb.ToString().Trim();
    }
    public static string GetEmployee147(SoftUniContext context)
    {
        var employee = context.Employees.Include(e => e.EmployeesProjects).ThenInclude(ep => ep.Project)
               .FirstOrDefault(e => e.EmployeeId == 147);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"{employee.FirstName} {employee.LastName} - {employee.JobTitle}");

        foreach (var ep in employee.EmployeesProjects.OrderBy(x => x.Project.Name))
            sb.AppendLine(ep.Project.Name);

        return sb.ToString().Trim();
    }
    public static string GetDepartmentsWithMoreThan5Employees(SoftUniContext context)
    {
        StringBuilder sb = new StringBuilder();
        var departments = context.Departments.Where(x => x.Employees.Count > 5)
            .Select(x => new
            {
               DepartmentName = x.Name,
               ManagerFName = x.Manager.FirstName,
               ManagerLName = x.Manager.LastName,
               Employees = x.Employees
            })
             .OrderBy(x => x.Employees.Count).ThenBy(x => x.DepartmentName)
                .ToArray();
        foreach (var d in departments)
        {
            sb.AppendLine($"{d.DepartmentName} - {d.ManagerFName} {d.ManagerLName}");
            foreach (var e in d.Employees.OrderBy(x => x.FirstName).ThenBy(x => x.LastName))
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} - {e.JobTitle}");
            }
        }
        return sb.ToString().Trim();
    }
    public static string GetLatestProjects(SoftUniContext context)
    {
        StringBuilder sb = new StringBuilder();
        var projects = context.Projects.Take(10).Select(x => new
        {
            x.Name,
            x.Description,
            x.StartDate
        }).OrderByDescending(x => x.StartDate).ToArray();
        foreach (var p in projects.OrderBy(x=>x.Name))
        {
            sb.AppendLine(p.Name)
            .AppendLine(p.Description)
            .AppendLine(p.StartDate.ToString("M/d/yyyy h:mm:ss tt"));
        }
        return sb.ToString().Trim();
    }
    public static string IncreaseSalaries(SoftUniContext context)
    {
        StringBuilder sb = new StringBuilder();
        var employees = context.Employees.Where(x => x.Department.Name == "Engineering" || x.Department.Name == "Tool Design" || x.Department.Name == "Marketing"
        || x.Department.Name == "Information Services")
            .Select(x => new
            {
                EmployeeFname = x.FirstName,
                EmployeeLName = x.LastName,
                DepartmentName = x.Department.Name,
                Salary = x.Salary + x.Salary * 12 / 100
            }).OrderBy(x => x.EmployeeFname).ThenBy(x => x.EmployeeLName).ToArray();
        foreach (var e in employees)
        {
            sb.AppendLine($"{e.EmployeeFname} {e.EmployeeLName} (${e.Salary:f2})");
        }
        return sb.ToString().Trim();

    }
    public static string GetEmployeesByFirstNameStartingWithSa(SoftUniContext context)
    {
        StringBuilder sb = new StringBuilder();
        var  employees  = context.Employees.Where(x=>x.FirstName.StartsWith("Sa")).Select(x => new
        {
            FirstName = x.FirstName,
            LastName = x.LastName,  
            JobTitle = x.JobTitle,
            Salary = x.Salary
        }).OrderBy(x=>x.FirstName).ThenBy(x => x.LastName).ToArray();
        foreach (var e in employees)
        {
            sb.AppendLine($"{e.FirstName} {e.LastName} - {e.JobTitle} - (${e.Salary:f2})");
        }
        return sb.ToString().Trim();
    }
    public static string DeleteProjectById(SoftUniContext context)
    {
        var projectToDelete = context.Projects.First(x => x.ProjectId == 2);

        var empProjectsToDelete = context.EmployeesProjects.Where(ep => ep.ProjectId == 2).ToArray();
        foreach (var empProject in empProjectsToDelete)
            context.EmployeesProjects.Remove(empProject);
            context.Projects.Remove(projectToDelete);
            context.SaveChanges();
        StringBuilder sb = new StringBuilder();
        foreach (var p in context.Projects.Take(10))
            sb.AppendLine(p.Name);
        return sb.ToString().TrimEnd();
    }
    public static string RemoveTown(SoftUniContext context)
    {
        var townToRemove = context.Towns.FirstOrDefault(t => t.Name == "Seattle");
        var addresses = context.Addresses.Where(a => a.TownId == townToRemove.TownId);
        var count = addresses.Count();
        var employees = context.Employees.Where(e => addresses.Any(a => a.AddressId == e.AddressId));
        foreach (var employee in employees) employee.AddressId = null;
        foreach (var address in addresses) context.Addresses.Remove(address);
        context.Towns.Remove(townToRemove);
        context.SaveChanges();
        return $"{count} addresses in Seattle were deleted";
    }
}