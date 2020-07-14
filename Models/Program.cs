﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Judge1.Models
{
    public enum Language
    {
        C99 = 50,
        C11 = 150,
        C14 = 250,
        CSharp = 51,
        Cpp03 = 54,
        Cpp11 = 154,
        Cpp14 = 254,
        Cpp17 = 354,
        Go = 60,
        Haskell = 61,
        Java8 = 27,
        Java13 = 62,
        JavaScript = 63,
        Lua = 64,
        Php = 68,
        Python3 = 71,
        Ruby = 72,
        Rust = 73,
        TypeScript = 74,
    }

    public enum Verdict
    {
        Failed = -1,
        Pending = 0,
        InQueue = 1,
        Running = 2,
        Accepted = 3,
        WrongAnswer = 4,
        TimeLimitExceeded = 5,
        CompilationError = 6,
        RuntimeErrorSigsegv = 7,
        RuntimeErrorSigxfsz = 8,
        RuntimeErrorSigfpe = 9,
        RuntimeErrorSigabrt = 10,
        RuntimeErrorNzec = 11,
        RuntimeErrorOther = 12,
        InternalError = 13,
        ExecFormatError = 14,
    }

    [NotMapped]
    public class Program
    {
        public Language Language { get; set; }
        public string Code { get; set; }
    }
}