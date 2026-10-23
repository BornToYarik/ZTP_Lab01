using System;
using System.Collections.Generic;

namespace ZTP_Lab01.Domain.Exceptions;

public class DomainValidationException : Exception
{
    public Dictionary<string, List<string>> Errors { get; }

    public DomainValidationException(Dictionary<string, List<string>> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}