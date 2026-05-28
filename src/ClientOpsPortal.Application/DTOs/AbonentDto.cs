using ClientOpsPortal.Application.DTOs.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ClientOpsPortal.Application.DTOs
{  
    public class AbonentDto : AuditableDto
    {
        public Guid UserId { get; set; }
        public required string IdentificationNumber { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string? MiddleName { get; set; }
        public required string AccountNumber { get; set; } 
    }

    public class AbonentShortDataDto
    {
        public Guid Id { get; set; }
        public required string AccountNumber { get; set; }
        public required string FullName { get; set; }
    }

    public class CreateAbonentDto
    {
        [Required(ErrorMessage = "Идентификационный номер обязателен")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Идентификационный номер должен содержать от 5 до 20 символов")]
        [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Идентификационный номер может содержать только заглавные буквы и цифры")]
        public required string IdentificationNumber { get; set; }

        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Имя должно содержать от 2 до 50 символов")]
        [RegularExpression(@"^[А-Яа-яA-Za-z\s\-]+$", ErrorMessage = "Имя может содержать только буквы, пробелы и дефисы")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Фамилия обязательна")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Фамилия должна содержать от 2 до 50 символов")]
        [RegularExpression(@"^[А-Яа-яA-Za-z\s\-]+$", ErrorMessage = "Фамилия может содержать только буквы, пробелы и дефисы")]
        public required string LastName { get; set; }

        [StringLength(50, ErrorMessage = "Отчество не может быть длиннее 50 символов")]
        [RegularExpression(@"^[А-Яа-яA-Za-z\s\-]*$", ErrorMessage = "Отчество может содержать только буквы, пробелы и дефисы")]
        public required string? MiddleName { get; set; }

        [StringLength(20, MinimumLength = 3, ErrorMessage = "Номер лицевого счета должен содержать от 3 до 20 символов")]
        [RegularExpression(@"^[A-Z0-9\-]+$", ErrorMessage = "Номер лицевого счета может содержать только заглавные буквы, цифры и дефисы")]
        public string? AccountNumber { get; set; }

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        public required string Email { get; set; }
    }

    public class UpdateAbonentDto : BaseDto
    {
        [Required(ErrorMessage = "Идентификационный номер обязателен")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Идентификационный номер должен содержать от 5 до 20 символов")]
        [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Идентификационный номер может содержать только заглавные буквы и цифры")]
        public required string IdentificationNumber { get; set; }

        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Имя должно содержать от 2 до 50 символов")]
        [RegularExpression(@"^[А-Яа-яA-Za-z\s\-]+$", ErrorMessage = "Имя может содержать только буквы, пробелы и дефисы")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Фамилия обязательна")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Фамилия должна содержать от 2 до 50 символов")]
        [RegularExpression(@"^[А-Яа-яA-Za-z\s\-]+$", ErrorMessage = "Фамилия может содержать только буквы, пробелы и дефисы")]
        public required string LastName { get; set; }

        [StringLength(50, ErrorMessage = "Отчество не может быть длиннее 50 символов")]
        [RegularExpression(@"^[А-Яа-яA-Za-z\s\-]*$", ErrorMessage = "Отчество может содержать только буквы, пробелы и дефисы")]
        public required string? MiddleName { get; set; }

        [Required(ErrorMessage = "Номер лицевого счета обязателен")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Номер лицевого счета должен содержать от 3 до 20 символов")]
        [RegularExpression(@"^[A-Z0-9\-]+$", ErrorMessage = "Номер лицевого счета может содержать только заглавные буквы, цифры и дефисы")]
        public required string AccountNumber { get; set; }
    }
}
