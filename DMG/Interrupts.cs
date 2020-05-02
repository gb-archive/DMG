﻿using System;
namespace DMG
{
    public class Interrupts
    {
        public bool ResumeCpuWhenInterruptBecomesPending { get; set; }

        public enum Interrupt
        {
            INTERRUPTS_VBLANK = (1 << 0),
            INTERRUPTS_LCDSTAT = (1 << 1),
            INTERRUPTS_TIMER = (1 << 2),
            INTERRUPTS_SERIAL = (1 << 3),
            INTERRUPTS_JOYPAD = (1 << 4)
        }

        //public readonly byte INTERRUPTS_VBLANK =  (1 << 0);
        //public readonly byte INTERRUPTS_LCDSTAT = (1 << 1);
        //public readonly byte INTERRUPTS_TIMER =   (1 << 2);
        //public readonly byte INTERRUPTS_SERIAL =  (1 << 3);
        //public readonly byte INTERRUPTS_JOYPAD =  (1 << 4);

        public bool InterruptsMasterEnable { get; set; }


        // Bitfield to tell us which interupts are enabled
        public byte InterruptEnableRegister { get; set; }

        // https://gbdev.gg8.se/wiki/articles/Interrupts
        // When an interrupt is enabled (above) the it is fired by setting it in these flags
        public byte InterruptFlags { get; set; }


        DmgSystem dmg;

        public Interrupts(DmgSystem dmg)
        {
            this.dmg = dmg;            
        }

        public void Reset()
        {
            InterruptsMasterEnable = false;
            InterruptFlags = 0;
            InterruptEnableRegister = 0;
        }


        public bool RequestInterrupt(Interrupt interrupt)
        {
            if ((InterruptEnableRegister & (byte) interrupt) == 0)
            {
                return false;                
            }

            InterruptFlags |= (byte) interrupt;
            
            // This is set by the HALT instruction
            if(ResumeCpuWhenInterruptBecomesPending)
            {
                ResumeCpuWhenInterruptBecomesPending = false;
                dmg.cpu.IsHalted = false;
            }
            return true;
        }

        public bool IsAnInterruptPending()
        {
            return ((InterruptEnableRegister & InterruptFlags) != 0);
        }


        public void Step()
        {
            if (InterruptsMasterEnable && InterruptEnableRegister != 0 && InterruptFlags != 0)
            {
                byte fire = (byte) (InterruptEnableRegister & InterruptFlags);


                // TODO: check the priority order below


                byte inter = (byte)(Interrupt.INTERRUPTS_VBLANK);
                if ((fire & inter) != 0)
                {
                    InterruptFlags &= (byte)~inter;
                    vblank();
                    return;
                }

                inter = (byte)(Interrupt.INTERRUPTS_LCDSTAT);
                if ((fire & inter) != 0)
                {
                    InterruptFlags &= (byte)~inter;
                    lcdStat();
                    return;
                }

                inter = (byte)(Interrupt.INTERRUPTS_TIMER);
                if ((fire & inter) != 0)
                {
                    InterruptFlags &= (byte)~inter;
                    timer();
                    return;
                }

                inter = (byte)(Interrupt.INTERRUPTS_SERIAL);
                if ((fire & inter) != 0)
                {
                    InterruptFlags &= (byte)~inter;
                    serial();
                    return;
                }

                inter = (byte)(Interrupt.INTERRUPTS_JOYPAD);
                if ((fire & inter) != 0)
                {
                    InterruptFlags &= (byte)~inter;
                    joypad();
                    return;
                }          
            }
        }



        void vblank()
        {
            //drawFramebuffer();
            //if (dmg.OnFrame != null)
            //{
            //    dmg.OnFrame();
            //}

            InterruptsMasterEnable = false;
            dmg.cpu.IsHalted = false;
            dmg.cpu.StackPush(dmg.cpu.PC);
            dmg.cpu.PC = 0x40;
            dmg.cpu.PeekNextInstruction();


            dmg.cpu.Ticks += 12;            
        }


        void lcdStat()
        {
            InterruptsMasterEnable = false;
            dmg.cpu.IsHalted = false;
            dmg.cpu.StackPush(dmg.cpu.PC);
            dmg.cpu.PC = 0x48;
            dmg.cpu.PeekNextInstruction();

            dmg.cpu.Ticks += 12;
        }


        void timer()
        {
            InterruptsMasterEnable = false;
            dmg.cpu.IsHalted = false;
            dmg.cpu.StackPush(dmg.cpu.PC);
            dmg.cpu.PC = 0x50;
            dmg.cpu.PeekNextInstruction();

            dmg.cpu.Ticks += 12;
        }


        void serial()
        {
            InterruptsMasterEnable = false;
            dmg.cpu.IsHalted = false;
            dmg.cpu.StackPush(dmg.cpu.PC);
            dmg.cpu.PC = 0x58;
            dmg.cpu.PeekNextInstruction();

            dmg.cpu.Ticks += 12;
        }


        void joypad()
        {
            InterruptsMasterEnable = false;
            dmg.cpu.IsHalted = false;
            dmg.cpu.StackPush(dmg.cpu.PC);
            dmg.cpu.PC = 0x60;
            dmg.cpu.PeekNextInstruction();

            dmg.cpu.Ticks += 12;
        }


        public void ReturnFromInterrupt()
        {
            InterruptsMasterEnable = true;
            dmg.cpu.PC = dmg.cpu.StackPop();
            dmg.cpu.PeekNextInstruction();
        }
    }
}
