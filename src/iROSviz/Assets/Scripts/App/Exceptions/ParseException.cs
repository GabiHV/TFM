using UnityEngine;

using System;

namespace App.Exceptions
{
    public class CastException : Exception
    {
        public string message;

        public CastException()
        {
        }

        public CastException(string message) : base(message)
        {
            this.message = message;
        }

        public CastException(string message, Exception inner) : base(message, inner)
        {
            this.message = message;
        }
    }

    public class IntegerCastException : CastException
    {
        public IntegerCastException()
        {     
        }

        public IntegerCastException(string message) : base(message)
        {
            this.message = message;
        }

        public IntegerCastException(string message, Exception inner) : base(message, inner)
        {
            this.message = message;
        }
    }

    public class FloatCastException : CastException
    {
        public FloatCastException()
        {     
        }

        public FloatCastException(string message) : base(message)
        {
            this.message = message;
        }

        public FloatCastException(string message, Exception inner) : base(message, inner)
        {
            this.message = message;
        }
    } 

    public class DoubleCastException : CastException
    {
        public DoubleCastException()
        {
        }

        public DoubleCastException(string message) : base(message)
        {
            this.message = message;
        }

        public DoubleCastException(string message, Exception inner) : base(message, inner)
        {
            this.message = message;
        }
    }   

    public class LongCastException : CastException
    {
        public LongCastException()
        {
        }

        public LongCastException(string message) : base(message)
        {
            this.message = message;
        }

        public LongCastException(string message, Exception inner) : base(message, inner)
        {
            this.message = message;
        }
    }   

    public class ByteCastException : CastException
    {
        public ByteCastException()
        {
        }

        public ByteCastException(string message) : base(message)
        {
            this.message = message;
        }

        public ByteCastException(string message, Exception inner) : base(message, inner)
        {
            this.message = message;
        }
    } 

    public class SByteCastException : CastException
    {
        public SByteCastException()
        {
        }

        public SByteCastException(string message) : base(message)
        {
            this.message = message;
        }

        public SByteCastException(string message, Exception inner) : base(message, inner)
        {
            this.message = message;
        }
    }   

    public class ByteArrayCastException : CastException
    {
        public ByteArrayCastException()
        {
        }

        public ByteArrayCastException(string message) : base(message)
        {
            this.message = message;
        }

        public ByteArrayCastException(string message, Exception inner) : base(message, inner)
        {
            this.message = message;
        }
    }   

    public class SByteArrayCastException : CastException
    {
        public SByteArrayCastException()
        {
        }

        public SByteArrayCastException(string message) : base(message)
        {
            this.message = message;
        }

        public SByteArrayCastException(string message, Exception inner) : base(message, inner)
        {
            this.message = message;
        }
    }   

    public class BoolCastException : CastException
    {
        public BoolCastException()
        {
        }

        public BoolCastException(string message) : base(message)
        {
            this.message = message;
        }

        public BoolCastException(string message, Exception inner) : base(message, inner)
        {
            this.message = message;
        }
    } 

    public class BoolArrayCastException : CastException
    {
        public BoolArrayCastException()
        {
        }

        public BoolArrayCastException(string message) : base(message)
        {
            this.message = message;
        }

        public BoolArrayCastException(string message, Exception inner) : base(message, inner)
        {
            this.message = message;
        }
    }  

    public class DoubleArrayCastException : CastException
    {
        public DoubleArrayCastException()
        {
        }

        public DoubleArrayCastException(string message) : base(message)
        {
            this.message = message;
        }

        public DoubleArrayCastException(string message, Exception inner) : base(message, inner)
        {
            this.message = message;
        }
    }   

    public class FloatArrayCastException : CastException
    {
        public FloatArrayCastException()
        {
        }

        public FloatArrayCastException(string message) : base(message)
        {
            this.message = message;
        }

        public FloatArrayCastException(string message, Exception inner) : base(message, inner)
        {
            this.message = message;
        }
    }  

    public class LongArrayCastException : CastException
    {
        public LongArrayCastException()
        {
        }

        public LongArrayCastException(string message) : base(message)
        {
            this.message = message;
        }

        public LongArrayCastException(string message, Exception inner) : base(message, inner)
        {
            this.message = message;
        }
    }   

    public class ArrayMalformed : CastException
    {
        public ArrayMalformed()
        {
        }

        public ArrayMalformed(string message) : base(message)
        {
            this.message = message;
        }

        public ArrayMalformed(string message, Exception inner) : base(message, inner)
        {
            this.message = message;
        }
    }   
}

