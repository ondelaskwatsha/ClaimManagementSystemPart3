# ClaimManagementSystem

#  Rosebank College - Contract Monthly Claim System


![PART 3 Background](https://github.com/user-attachments/assets/d5bb50b2-1645-4fe5-b195-68c85151eb8a)


Description
The Contract Monthly Claim System (CMCS) is a .NET-based web application designed to streamline the process of submitting, verifying, and approving claims by lecturers. This system supports multiple roles, including Lecturers, Programme Coordinators, and Academic Managers. The UI is implemented using WPF (.NET Core) as a front-end prototype and follows the Model View Controller (MVC) design pattern.

Project Structure
This project consists of the following parts:

User Interface (WPF Prototype): A visually appealing front-end design using .NET Core and WPF. The current version is a non-functional prototype focused on front-end design with no back-end functionality.
UML Class Diagram: A class diagram representing the data structures of the CMCS system, showing relationships between entities such as Claim, Lecturer, and Coordinator.
Project Plan: Detailed project plan outlining tasks, timelines, and deadlines for building the full prototype.
Future Improvements
Add backend functionality to make the system fully operational.
Implement role-based authentication using ASP.NET Identity.
Enhance error handling and validation to improve user experience.

##Features

### Role-Based Access Control
- Lecturers: Submit claims, track status, upload documents
- Program Coordinators: Review and verify claims
- Academic Managers: Approve claims and manage workflows
- HR Managers: Process payments and generate reports

### Core Functionality
- Automated Claim Submission with real-time validation
- Document Upload with file type and size restrictions
- Status Tracking with real-time updates
- PDF Report Generation for payments and analytics
- Role-Based Dashboard with customized views
- Payment Processing workflow automation

## Technology Stack

- Frontend: WPF (XAML/C#)
- Backend: .NET Core 6.0
- Data Storage: JSON File System + Entity Framework
- PDF Generation: iTextSharp
- Testing: MSTest
- Version Control: Git/GitHub

