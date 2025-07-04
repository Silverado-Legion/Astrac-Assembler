# Astrac Assembler
Custom assembler to assemble your custom instruction sets!
Usage is simple. Place the .isa file (the file containing the instruction set) and the .asm file to be assembled in the same folder, and run the assembler in that folder. It will automatically find the files and assemble them.

# Usage
The format for writing an instruction set for your custom computer is somewhat inspired by the system used by the game Turing Complete, but with a slightly different and more strict syntax. This file format is also just plain text. First, start with 6 tags:
1. $ASTRAC_V = 1.0.0
2. $ASTRAC_OUT_TYPE = .filenamehere
3. [Name]
4. [Description]
5. [Registers]
6. [Instructions.]
With these tags, you can begin writing the instruction set for the assembler. $ASTRAC_V stores the version of the assembler it requires (not the version of the instruction set itself). This is required for the file to be recognized and used by the assembler. $ASTRAC_OUT_TYPE stores the file extension for the finished binary files, this is mostly arbitrary. Follow these woth [Name], which is to be followed on the next line with the name of the instruction set or computer. Then, [Description], which is to be followed on the next line with the description of the computer or instruction set. Now, the final two tags contain the actual technical information the assembler needs. These are [Registers] and [Instructions]. Under [Registers], write the aliases for the registers you have in your custom computer, one on each line. Following each alias (on the same line), you can write the hexadecimal number which represents the register. When you write the alias for a register in the assembly file, the assembler will swap out the alias for the hexadecimal number in binary. Finally, the [Instructions] tag. For each instruction, write the alias, parameters, then underneath that write the actual opcode (in hexadecimal). For example:
```
imm %r %v
010r vvvv
; - Immediate value %v to register %r
```
As you can see, parameters are represented with single letters, which is prefixed with a precent sign (%). The number of instances of each parameter letter is the size of that parameter. To understand this, let's try it out. We want to immediate the number 241 to the first general purpose register in our instruction set, which is ra (in the set I'm using as the example). So, we write `imm ra  241`. The assembler taks this, and it starts turning it into machinecode. First, it recognizes `imm` as `010r vvvv`, and then it starts replacing those letters. The first parameter is `%r`, which we filled in with `ra`. It looks as the isa, and finds this: `ra 3 ; General-purpose A`, so  it knows to replace `r` with `3`. At this point, we have `1013 vvvv`, and now it considers the second parameter, the number. It recognizes `241` as decimal, so it converts it to hexadecimal, and fills in the space left by `vvvv`. So `241` gets converted to `00F1` (4 digits because it has to fill a 4 hexadecimal digit space). So we get `0103 00F1`, which it can now enscribe into the binary file. Now, a few bits of extra information to keep in mind:
* Instructions do not need to have parameters.
* Comments start with `;`
* Hexadecimal numbers MUST be written in uppercase in the .isa file.
* Hexadecimal numbers in the .asm files MUST start with either 0x or $.
* Binary numbers in the .asm files MUST start with either 0b or %.
* The number of digits in the instruction opcode matters. All instructions must have words of the same lengh, and multiple words can be seperated by spaces (for longer instructions). For example, the `imm` instruction mentioned earlier is a 2 word instruction, as it is `010r vvvv`, with each word being 4 hexadecimal digits, or 16-bit.
* Some errors may not be logged by the assembler, leading to a false report of success. This is rare however, and shouldn't occur at all if this usage guide is followed.
