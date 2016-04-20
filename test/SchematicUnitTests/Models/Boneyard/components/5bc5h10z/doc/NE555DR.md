# xx555 Precision Timers
## NE555DR
### Semiconductors and Actives › Clock, Timing › Timers 
***

#### Description
These devices are precision timing circuits capable of producing accurate time delays or oscillation. In the time-delay or mono-stable mode of operation, the timed interval is controlled by a single external resistor and capacitor network. In the a-stable mode of operation, the frequency and duty cycle can be controlled independently with two external resistors and a single external capacitor.

The threshold and trigger levels normally are two-thirds and one-third, respectively, of VCC. These levels can be altered by use of the control-voltage terminal. When the trigger input falls below the trigger level, the flip-flop is set, and the output goes high. If the trigger input is above the threshold level, the flip-flop is reset and the output is low The reset (RESET) input can override all other inputs and can be used to initiate a new timing cycle. When RESET goes low, the flip-flop is reset, and the output goes low. When the output is low, a low-impedance path is provided between discharge (DISCH) and ground.

The output circuit is capable of sinking or sourcing current up to 200 mA. Operation is specified for supplies of 5 V to 15 V.  With a 5-V supply, output levels are compatible with TTL inputs.

### Connectors 
- ***CONT* []:** Controls comparator thresholds, Outputs 2/3 VCC, allows bypass capacitor connection.
- ***DISCH* []:** Open collector output to discharge timing capacitor.
- ***OUT* []:** High current timer output signal.
- ***RESET* []:** Active low reset input forces output and discharge low.
- ***THRES* []:** End of timing input. THRES > CONT sets output low and discharge low.
- ***TRIG* []:** Start of timing input. TRIG < ½ CONT sets output high and discharge open.
- ***VCC* []:** Input supply voltage, 4.5 V to 16 V.
