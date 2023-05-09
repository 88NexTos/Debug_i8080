st:
	org 0

	nop

	lxi bc, 0x0102
	lxi de, 0x0304
	lxi hl, 0x0506
	lxi sp, 0x0708

	stax bc
	stax de
	shld 0x090A
	sta 0x0B0C

	inx bc
	inx de
	inx hl
	inx sp

	dad bc
	dad de
	dad hl
	dad sp

	dcx bc
	dcx de
	dcx hl
	dcx sp

	ldax bc
	ldax de
	lhld 0x0D0E
	lda 0x0F00

	mvi b, 0x10
	mvi c, 0x11
	mvi d, 0x12
	mvi e, 0x13
	mvi h, 0x14
	mvi l, 0x15
	
	mvi m, 0x16
	mvi a, 0x17

	inr b
	inr c
	inr d
	inr e
	inr h
	inr l

	inr m
	inr a

	rlc
	rrc
	ral
	rar
	daa
	cma
	stc
	cmc

	dcr b
	dcr c
	dcr d
	dcr e
	dcr h
	dcr l

	dcr m
	dcr a

	adi 0x18
	aci 0x19
	sui 0x1A
	sbi 0x1B
	ani 0x1C
	xri 0x1D
	ori 0x1E
	cpi 0x1F

	push bc
	push de
	push hl
	push psw

	xthl
	xchg

	di
	in 0xFE
	out 0xED
	ei

	pop bc
	pop de
	pop hl
	pop psw

	call rr
	jmp r1
rr:	
	rst 0
	rst 1
	rst 2
	rst 3
	rst 4
	rst 5
	rst 6
	rst 7
	ret
r1:
	sphl
	pchl

	jmp st	; start
